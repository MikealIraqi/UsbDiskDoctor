using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Recovery.Models;
using UsbDiskDoctor.Recovery.Restoring;
using UsbDiskDoctor.Recovery.Scanning;

namespace UsbDiskDoctor.Recovery.Services
{
    /// <summary>
    /// Orchestrates file recovery using mounted volume scanning and safe file restoring.
    /// Read-only on source. Restore requires target on a different volume.
    /// </summary>
    public sealed class MountedVolumeRecoveryService : IRecoveryService
    {
        private static readonly ILogger _log = Log.ForContext<MountedVolumeRecoveryService>();

        private readonly IVolumeScanner _scanner;
        private readonly IFileRestorer _restorer;

        public MountedVolumeRecoveryService(IVolumeScanner scanner, IFileRestorer restorer)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
            _restorer = restorer ?? throw new ArgumentNullException(nameof(restorer));
        }

        public async Task<IReadOnlyList<RecoveredFileInfo>> ScanAsync(
            string sourceVolume,
            RecoveryOptions options,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceVolume))
            {
                throw new ArgumentException("Source volume is required.", nameof(sourceVolume));
            }

            var normalizedRoot = NormalizeVolumeRoot(sourceVolume);

            _log.Information("Recovery scan started on {VolumeRoot}.", normalizedRoot);

            var results = await _scanner.ScanAsync(
                normalizedRoot,
                options ?? new RecoveryOptions(),
                progress,
                cancellationToken);

            _log.Information("Recovery scan finished: {Count} files.", results.Count);
            return results;
        }

        public async Task<RecoverySession> RestoreAsync(
            string sourceVolume,
            IReadOnlyList<RecoveredFileInfo> files,
            string targetFolder,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceVolume))
            {
                throw new ArgumentException("Source volume is required.", nameof(sourceVolume));
            }

            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            if (string.IsNullOrWhiteSpace(targetFolder))
            {
                throw new ArgumentException("Target folder is required.", nameof(targetFolder));
            }

            var normalizedRoot = NormalizeVolumeRoot(sourceVolume);

            _log.Information(
                "Recovery restore started: {Count} files from {Source} to {Target}.",
                files.Count, normalizedRoot, targetFolder);

            var session = await _restorer.RestoreAsync(
                normalizedRoot,
                files,
                targetFolder,
                progress,
                cancellationToken);

            _log.Information(
                "Recovery restore finished. Status={Status}, Recovered={Recovered}.",
                session.Status, session.RecoveredFilesCount);

            return session;
        }

        private static string NormalizeVolumeRoot(string volume)
        {
            var trimmed = volume.Trim();

            // "E:" → "E:\"
            if (trimmed.Length == 2 && trimmed[1] == ':')
            {
                return trimmed + "\\";
            }

            // "E:\" or "E:\something"
            if (!trimmed.EndsWith('\\'))
            {
                trimmed += '\\';
            }

            return trimmed;
        }
    }
}