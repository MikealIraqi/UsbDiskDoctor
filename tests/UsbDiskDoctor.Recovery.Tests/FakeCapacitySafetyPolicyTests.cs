using System;
using System.IO;
using UsbDiskDoctor.Recovery.FakeCapacity;
using Xunit;

namespace UsbDiskDoctor.Recovery.Tests;

public sealed class FakeCapacitySafetyPolicyTests
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

    private static DriveInfoEx MakeDrive(
        char letter = 'Z',
        string busType = "USB",
        long totalBytes = 16L * 1024 * 1024 * 1024,
        DriveType driveType = DriveType.Removable)
    {
        return new DriveInfoEx
        {
            DriveLetter = letter,
            VolumeLabel = "TestUSB",
            TotalSizeBytes = totalBytes,
            FreeSpaceBytes = totalBytes,
            DriveType = driveType,
            DriveFormat = "FAT32",
            BusType = busType
        };
    }

    [Fact]
    public void Validate_WithEmptyDriveLetter_ReturnsUnsafe()
    {
        var drive = MakeDrive(letter: '\0');
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void Validate_WithInvalidCharacterAsDriveLetter_ReturnsUnsafe()
    {
        var drive = MakeDrive(letter: '1');
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void Validate_WithSystemDriveLetter_ReturnsUnsafe()
    {
        var systemLetter = char.ToUpperInvariant(Environment.SystemDirectory[0]);
        var drive = MakeDrive(letter: systemLetter);
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void Validate_WithNonUsbBusType_ReturnsUnsafe()
    {
        var drive = MakeDrive(letter: GetSafeTestDriveLetter(), busType: "SCSI");
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void Validate_WithZeroSize_ReturnsUnsafe()
    {
        var drive = MakeDrive(letter: GetSafeTestDriveLetter(), totalBytes: 0);
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void Validate_WithNegativeSize_ReturnsUnsafe()
    {
        var drive = MakeDrive(letter: GetSafeTestDriveLetter(), totalBytes: -1);
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void Validate_WithValidUsbDrive_ReturnsSafe()
    {
        var safeLetter = GetSafeTestDriveLetter();
        var drive = MakeDrive(letter: safeLetter, busType: "USB");
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.True(result.IsSafe);
    }

    [Fact]
    public void Validate_WithLowercaseUsbBusType_ReturnsSafe()
    {
        var safeLetter = GetSafeTestDriveLetter();
        var drive = MakeDrive(letter: safeLetter, busType: "usb");
        var result = FakeCapacitySafetyPolicy.Validate(drive);
        Assert.True(result.IsSafe);
    }
}