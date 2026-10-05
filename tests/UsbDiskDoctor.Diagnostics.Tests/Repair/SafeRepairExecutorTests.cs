using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Repair.Execution;
using Xunit;

namespace UsbDiskDoctor.Diagnostics.Tests.Repair
{
    /// <summary>
    /// In-memory fake command runner. Never launches a real process.
    /// </summary>
    internal sealed class FakeCommandRunner : ICommandRunner
    {
        private readonly CommandRunResult _result;

        public int CallCount { get; private set; }
        public string? LastExecutable { get; private set; }
        public string? LastArguments { get; private set; }

        public FakeCommandRunner(CommandRunResult result)
        {
            _result = result;
        }

        public Task<CommandRunResult> RunAsync(
            string executable,
            string arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastExecutable = executable;
            LastArguments = arguments;
            return Task.FromResult(_result);
        }
    }

    /// <summary>
    /// Unit tests for <see cref="SafeRepairExecutor"/>.
    /// No real processes are launched.
    /// </summary>
    public class SafeRepairExecutorTests
    {
        private static RepairProposal MakeProposal(
            string actionCode,
            RiskLevel riskLevel,
            bool requiresConfirmation = false)
        {
            return new RepairProposal
            {
                ActionCode = actionCode,
                RiskLevel = riskLevel,
                RequiresConfirmation = requiresConfirmation,
                DescriptionAr = "وصف",
                DescriptionEn = "Description"
            };
        }

        private static CommandRunResult SuccessResult() => new()
        {
            StartedSuccessfully = true,
            ExitCode = 0,
            StdOut = "ok",
            StdErr = "",
            TimedOut = false,
            Duration = TimeSpan.FromMilliseconds(10)
        };

        private static CommandRunResult FailureResult() => new()
        {
            StartedSuccessfully = true,
            ExitCode = 1,
            StdOut = "",
            StdErr = "error",
            TimedOut = false,
            Duration = TimeSpan.FromMilliseconds(10)
        };

        [Fact]
        public async Task ExecuteAsync_ThrowsArgumentNullException_OnNullProposal()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.ExecuteAsync(null!, null, null));
        }

        [Fact]
        public async Task ExecuteAsync_RejectsNonWhitelistedAction()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("SOMETHING_RANDOM", RiskLevel.Safe);

            var result = await sut.ExecuteAsync(proposal, null, null);

            Assert.False(result.Executed);
            Assert.Equal("NOT_WHITELISTED", result.RejectionCode);
            Assert.Equal(0, runner.CallCount);
        }

        [Fact]
        public async Task ExecuteAsync_RejectsRiskMismatch()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            // DIRTY_BIT_SET requires Medium, but we claim Safe
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Safe);

            var result = await sut.ExecuteAsync(proposal, "CONFIRM", "E:");

            Assert.False(result.Executed);
            Assert.Equal("RISK_MISMATCH", result.RejectionCode);
            Assert.Equal(0, runner.CallCount);
        }

        [Fact]
        public async Task ExecuteAsync_RejectsMissingToken()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Medium);

            var result = await sut.ExecuteAsync(proposal, confirmationToken: null, targetDriveLetter: "E:");

            Assert.False(result.Executed);
            Assert.Equal("MISSING_TOKEN", result.RejectionCode);
            Assert.Equal(0, runner.CallCount);
        }

        [Fact]
        public async Task ExecuteAsync_RejectsWrongToken_CaseSensitive()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Medium);

            // "confirm" (lowercase) must NOT match "CONFIRM"
            var result = await sut.ExecuteAsync(proposal, confirmationToken: "confirm", targetDriveLetter: "E:");

            Assert.False(result.Executed);
            Assert.Equal("MISSING_TOKEN", result.RejectionCode);
        }

        [Fact]
        public async Task ExecuteAsync_AcceptsSafeInformationalAction_NoCommandRun()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("OPERATIONAL_STATUS_ERROR", RiskLevel.Safe);

            var result = await sut.ExecuteAsync(proposal, null, null);

            Assert.True(result.Executed);
            Assert.True(result.Succeeded);
            Assert.Equal(0, runner.CallCount);  // informational action — no process
        }

        [Fact]
        public async Task ExecuteAsync_RejectsMissingDriveLetter_WhenRequired()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Medium);

            var result = await sut.ExecuteAsync(proposal, "CONFIRM", targetDriveLetter: null);

            Assert.False(result.Executed);
            Assert.Equal("MISSING_DRIVE_LETTER", result.RejectionCode);
            Assert.Equal(0, runner.CallCount);
        }

        [Fact]
        public async Task ExecuteAsync_RunsWhitelistedCommand_OnSuccessPath()
        {
            var runner = new FakeCommandRunner(SuccessResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Medium);

            var result = await sut.ExecuteAsync(proposal, "CONFIRM", "E:");

            Assert.True(result.Executed);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.CommandResult);
            Assert.Equal(1, runner.CallCount);
            Assert.Contains("chkdsk.exe", runner.LastExecutable);
            Assert.Contains("E:", runner.LastArguments);
            Assert.Contains("/scan", runner.LastArguments);
        }

        [Fact]
        public async Task ExecuteAsync_ReportsFailure_WhenCommandExitCodeNonZero()
        {
            var runner = new FakeCommandRunner(FailureResult());
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Medium);

            var result = await sut.ExecuteAsync(proposal, "CONFIRM", "E:");

            Assert.True(result.Executed);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.CommandResult);
            Assert.Equal(1, result.CommandResult!.ExitCode);
        }

        [Fact]
        public async Task ExecuteAsync_ReportsFailure_WhenCommandTimesOut()
        {
            var timeoutResult = new CommandRunResult
            {
                StartedSuccessfully = true,
                TimedOut = true,
                ExitCode = -1,
                Duration = TimeSpan.FromSeconds(30)
            };
            var runner = new FakeCommandRunner(timeoutResult);
            var sut = new SafeRepairExecutor(runner);
            var proposal = MakeProposal("DIRTY_BIT_SET", RiskLevel.Medium);

            var result = await sut.ExecuteAsync(proposal, "CONFIRM", "E:");

            Assert.True(result.Executed);
            Assert.False(result.Succeeded);
        }
    }
}