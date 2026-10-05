using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace UsbDiskDoctor.Repair.Execution
{
    /// <summary>
    /// Executes external commands using System.Diagnostics.Process with security hardening.
    /// </summary>
    public sealed class ProcessCommandRunner : ICommandRunner
    {
        private const int MaxOutputChars = 8192;
        private static readonly ILogger _log = Log.ForContext<ProcessCommandRunner>();

        public async Task<CommandRunResult> RunAsync(
            string executable,
            string arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(executable) || string.IsNullOrWhiteSpace(arguments))
            {
                return new CommandRunResult
                {
                    StartedSuccessfully = false,
                    FailureReason = "Executable or arguments are empty."
                };
            }

            var stopwatch = Stopwatch.StartNew();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            try
            {
                using var process = new Process { StartInfo = processStartInfo };

                if (!process.Start())
                {
                    return new CommandRunResult
                    {
                        StartedSuccessfully = false,
                        FailureReason = "Process.Start returned false.",
                        Duration = stopwatch.Elapsed
                    };
                }

                var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
                var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

                var waitTask = process.WaitForExitAsync(cancellationToken);
                var completedTask = await Task.WhenAny(waitTask, Task.Delay(timeout, cancellationToken));

                var timedOut = (completedTask != waitTask);

                if (timedOut)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch (Exception killException)
                    {
                        _log.Warning(killException, "Failed to kill process tree for {Executable}.", executable);
                    }

                    var partialOut = await SafeReadAsync(stdoutTask);
                    var partialErr = await SafeReadAsync(stderrTask);

                    stopwatch.Stop();
                    return new CommandRunResult
                    {
                        StartedSuccessfully = true,
                        TimedOut = true,
                        ExitCode = -1,
                        StdOut = Truncate(partialOut),
                        StdErr = Truncate(partialErr),
                        Duration = stopwatch.Elapsed
                    };
                }

                await waitTask;
                var outText = await stdoutTask;
                var errText = await stderrTask;

                stopwatch.Stop();
                return new CommandRunResult
                {
                    StartedSuccessfully = true,
                    ExitCode = process.ExitCode,
                    StdOut = Truncate(outText),
                    StdErr = Truncate(errText),
                    TimedOut = false,
                    Duration = stopwatch.Elapsed
                };
            }
            catch (OperationCanceledException)
            {
                _log.Information("Command execution cancelled for {Executable}.", executable);
                stopwatch.Stop();
                return new CommandRunResult
                {
                    StartedSuccessfully = true,
                    FailureReason = "Cancelled.",
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Win32Exception win32Exception)
            {
                _log.Error(win32Exception, "Win32 error while starting {Executable}: {Message}", executable, win32Exception.Message);
                stopwatch.Stop();
                return new CommandRunResult
                {
                    StartedSuccessfully = false,
                    FailureReason = win32Exception.Message,
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception genericException)
            {
                _log.Error(genericException, "Unexpected error while executing {Executable}.", executable);
                stopwatch.Stop();
                return new CommandRunResult
                {
                    StartedSuccessfully = false,
                    FailureReason = genericException.Message,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        private static string Truncate(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            if (input.Length <= MaxOutputChars)
            {
                return input;
            }

            return input.Substring(0, MaxOutputChars) + "\n...[truncated]";
        }

        private static async Task<string> SafeReadAsync(Task<string> readTask)
        {
            try
            {
                return await readTask;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}