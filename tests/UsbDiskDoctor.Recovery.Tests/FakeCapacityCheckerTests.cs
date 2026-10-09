using System;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Recovery.FakeCapacity;
using UsbDiskDoctor.Recovery.Tests.TestDoubles;
using Xunit;

namespace UsbDiskDoctor.Recovery.Tests;

public sealed class FakeCapacityCheckerTests
{
    private static char GetSafeTestDriveLetter()
    {
        var systemLetter = char.ToUpperInvariant(Environment.SystemDirectory[0]);
        for (var c = 'Z'; c >= 'D'; c--)
        {
            if (c != systemLetter)
            {
                return c;
            }
        }
        return 'Z';
    }

    private static DriveInfoEx MakeSafeDrive(char? letter = null)
    {
        var driveLetter = letter ?? GetSafeTestDriveLetter();
        return new DriveInfoEx
        {
            DriveLetter = driveLetter,
            VolumeLabel = "TestUSB",
            TotalSizeBytes = 16L * 1024 * 1024 * 1024,
            FreeSpaceBytes = 16L * 1024 * 1024 * 1024,
            DriveType = DriveType.Removable,
            DriveFormat = "FAT32",
            BusType = "USB"
        };
    }

    [Fact]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FakeCapacityChecker(null!));
    }

    [Fact]
    public async Task CheckAsync_WithUnknownDrive_ReturnsErrorResult()
    {
        var provider = new FakeDriveInfoProvider(Array.Empty<DriveInfoEx>());
        var checker = new FakeCapacityChecker(provider);
        var request = new FakeCapacityRequest { DriveLetter = 'Z' };

        var result = await checker.CheckAsync(request, null, CancellationToken.None);

        Assert.Equal(FakeCapacityStatus.Error, result.Status);
        Assert.Contains("not found", result.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckAsync_WithUnsafeDrive_ReturnsErrorResult()
    {
        var unsafeDrive = new DriveInfoEx
        {
            DriveLetter = GetSafeTestDriveLetter(),
            VolumeLabel = "TestSCSI",
            TotalSizeBytes = 16L * 1024 * 1024 * 1024,
            FreeSpaceBytes = 16L * 1024 * 1024 * 1024,
            DriveType = DriveType.Removable,
            DriveFormat = "NTFS",
            BusType = "SCSI"
        };
        var provider = new FakeDriveInfoProvider(new[] { unsafeDrive });
        var checker = new FakeCapacityChecker(provider);
        var request = new FakeCapacityRequest { DriveLetter = unsafeDrive.DriveLetter };

        var result = await checker.CheckAsync(request, null, CancellationToken.None);

        Assert.Equal(FakeCapacityStatus.Error, result.Status);
        Assert.Contains("USB", result.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckAsync_WithInvalidFileSize_ReturnsErrorResult()
    {
        var safeDrive = MakeSafeDrive();
        var provider = new FakeDriveInfoProvider(new[] { safeDrive });
        var checker = new FakeCapacityChecker(provider);
        var request = new FakeCapacityRequest
        {
            DriveLetter = safeDrive.DriveLetter,
            FileSizeBytes = 0
        };

        var result = await checker.CheckAsync(request, null, CancellationToken.None);

        Assert.Equal(FakeCapacityStatus.Error, result.Status);
    }

    [Fact]
    public async Task CheckAsync_WithInsufficientSpace_ReturnsErrorResult()
    {
        var smallDrive = new DriveInfoEx
        {
            DriveLetter = GetSafeTestDriveLetter(),
            VolumeLabel = "SmallUSB",
            TotalSizeBytes = 16L * 1024 * 1024 * 1024,
            FreeSpaceBytes = 1024,
            DriveType = DriveType.Removable,
            DriveFormat = "FAT32",
            BusType = "USB"
        };
        var provider = new FakeDriveInfoProvider(new[] { smallDrive });
        var checker = new FakeCapacityChecker(provider);
        var request = new FakeCapacityRequest
        {
            DriveLetter = smallDrive.DriveLetter,
            FileSizeBytes = 64 * 1024 * 1024
        };

        var result = await checker.CheckAsync(request, null, CancellationToken.None);

        Assert.Equal(FakeCapacityStatus.Error, result.Status);
        Assert.Contains("space", result.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var safeDrive = MakeSafeDrive();
        var provider = new FakeDriveInfoProvider(new[] { safeDrive });
        var checker = new FakeCapacityChecker(provider);
        var request = new FakeCapacityRequest { DriveLetter = safeDrive.DriveLetter };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => checker.CheckAsync(request, null, cts.Token));
    }
}