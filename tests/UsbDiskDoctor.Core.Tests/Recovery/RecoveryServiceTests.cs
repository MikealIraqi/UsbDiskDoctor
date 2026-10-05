using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Recovery.Models;
using UsbDiskDoctor.Recovery.Restoring;
using UsbDiskDoctor.Recovery.Services;
using UsbDiskDoctor.Recovery.Scanning;
using Xunit;

namespace UsbDiskDoctor.Core.Tests.Recovery
{
    // ============================================================
    // Fake implementations (no file I/O)
    // ============================================================

    internal sealed class FakeScanner : IVolumeScanner
    {
        private readonly IReadOnlyList<RecoveredFileInfo> _files;
        public FakeScanner(IReadOnlyList<RecoveredFileInfo> files) => _files = files;

        public Task<IReadOnlyList<RecoveredFileInfo>> ScanAsync(
            string volumeRoot,
            RecoveryOptions options,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_files);
        }
    }

    internal sealed class FakeRestorer : IFileRestorer
    {
        public string? LastSourceRoot { get; private set; }
        public int LastFileCount { get; private set; }
        public string? LastTarget { get; private set; }

        public Task<RecoverySession> RestoreAsync(
            string sourceVolumeRoot,
            IReadOnlyList<RecoveredFileInfo> files,
            string targetFolder,
            IProgress<RecoveryProgressInfo>? progress,
            CancellationToken cancellationToken = default)
        {
            LastSourceRoot = sourceVolumeRoot;
            LastFileCount = files.Count;
            LastTarget = targetFolder;

            return Task.FromResult(new RecoverySession
            {
                SourceVolume = sourceVolumeRoot,
                TargetFolder = targetFolder,
                Status = RecoverySessionStatus.Completed,
                RecoveredFilesCount = files.Count
            });
        }
    }

    // ============================================================
    // SafeFileRestorer — safety validation tests
    // (No actual copies — validation runs BEFORE any file I/O)
    // ============================================================

    public class SafeFileRestorerSafetyTests
    {
        [Fact]
        public async Task RestoreAsync_ThrowsOnEmptySourceRoot()
        {
            var sut = new SafeFileRestorer();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                sut.RestoreAsync(string.Empty, Array.Empty<RecoveredFileInfo>(), @"D:\target"));
        }

        [Fact]
        public async Task RestoreAsync_ThrowsOnNullFiles()
        {
            var sut = new SafeFileRestorer();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.RestoreAsync(@"E:\", null!, @"D:\target"));
        }

        [Fact]
        public async Task RestoreAsync_ThrowsOnEmptyTarget()
        {
            var sut = new SafeFileRestorer();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                sut.RestoreAsync(@"E:\", Array.Empty<RecoveredFileInfo>(), string.Empty));
        }

        [Fact]
        public async Task RestoreAsync_RejectsTargetInsideSource()
        {
            var sut = new SafeFileRestorer();
            var session = await sut.RestoreAsync(
                @"C:\Source\",
                Array.Empty<RecoveredFileInfo>(),
                @"C:\Source\Recovered\");

            Assert.Equal(RecoverySessionStatus.Failed, session.Status);
            Assert.NotEmpty(session.Errors);
        }

        [Fact]
        public async Task RestoreAsync_RejectsSameVolumeRoot()
        {
            var sut = new SafeFileRestorer();
            var session = await sut.RestoreAsync(
                @"C:\Source\",
                Array.Empty<RecoveredFileInfo>(),
                @"C:\Other\");

            Assert.Equal(RecoverySessionStatus.Failed, session.Status);
        }
    }

    // ============================================================
    // MountedVolumeRecoveryService — orchestration tests
    // (Uses fakes, no real file access)
    // ============================================================

    public class MountedVolumeRecoveryServiceTests
    {
        [Fact]
        public void Constructor_ThrowsOnNullScanner()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MountedVolumeRecoveryService(null!, new FakeRestorer()));
        }

        [Fact]
        public void Constructor_ThrowsOnNullRestorer()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MountedVolumeRecoveryService(new FakeScanner(Array.Empty<RecoveredFileInfo>()), null!));
        }

        [Fact]
        public async Task ScanAsync_ThrowsOnEmptySourceVolume()
        {
            var sut = new MountedVolumeRecoveryService(
                new FakeScanner(Array.Empty<RecoveredFileInfo>()),
                new FakeRestorer());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                sut.ScanAsync(string.Empty, new RecoveryOptions(), null));
        }

        [Fact]
        public async Task ScanAsync_NormalizesShortVolumeName()
        {
            var fileList = new List<RecoveredFileInfo>
            {
                new() { FileName = "a.jpg", SourceFullPath = @"E:\a.jpg", SizeBytes = 100 }
            };
            var scanner = new FakeScanner(fileList);
            var sut = new MountedVolumeRecoveryService(scanner, new FakeRestorer());

            var results = await sut.ScanAsync("E:", new RecoveryOptions(), null);

            Assert.Single(results);
            Assert.Equal("a.jpg", results[0].FileName);
        }

        [Fact]
        public async Task RestoreAsync_ThrowsOnNullFiles()
        {
            var sut = new MountedVolumeRecoveryService(
                new FakeScanner(Array.Empty<RecoveredFileInfo>()),
                new FakeRestorer());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                sut.RestoreAsync(@"E:\", null!, @"D:\out"));
        }

        [Fact]
        public async Task RestoreAsync_DelegatesToRestorer_WithNormalizedRoot()
        {
            var restorer = new FakeRestorer();
            var sut = new MountedVolumeRecoveryService(
                new FakeScanner(Array.Empty<RecoveredFileInfo>()),
                restorer);

            var files = new List<RecoveredFileInfo>
            {
                new() { FileName = "x.jpg", SourceFullPath = @"E:\x.jpg" }
            };

            var session = await sut.RestoreAsync("E:", files, @"D:\out", null);

            Assert.Equal(@"E:\", restorer.LastSourceRoot);
            Assert.Equal(@"D:\out", restorer.LastTarget);
            Assert.Equal(1, restorer.LastFileCount);
            Assert.Equal(RecoverySessionStatus.Completed, session.Status);
        }
    }
}