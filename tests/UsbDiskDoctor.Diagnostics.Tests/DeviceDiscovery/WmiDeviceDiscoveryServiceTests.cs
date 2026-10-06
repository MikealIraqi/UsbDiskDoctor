// File: tests/UsbDiskDoctor.Diagnostics.Tests/DeviceDiscovery/WmiDeviceDiscoveryServiceTests.cs
using System;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Diagnostics.DeviceDiscovery;
using Xunit;

namespace UsbDiskDoctor.Diagnostics.Tests.DeviceDiscovery;

/// <summary>
/// Tests for the pure static helpers in WmiDeviceDiscoveryService that map raw WMI
/// values to the MediaType enum.
/// </summary>
public sealed class WmiDeviceDiscoveryServiceTests
{
    // === Group A: ExtractPhysicalDriveNumber ===

    [Theory]
    [InlineData(@"\\.\PHYSICALDRIVE0", "0")]
    [InlineData(@"\\.\PHYSICALDRIVE5", "5")]
    [InlineData(@"\\.\PHYSICALDRIVE123", "123")]
    [InlineData(@"\\.\physicaldrive7", "7")]
    public void ExtractPhysicalDriveNumber_ValidId_ReturnsNumber(string deviceId, string expected)
    {
        Assert.Equal(expected, WmiDeviceDiscoveryService.ExtractPhysicalDriveNumber(deviceId));
    }

    [Fact]
    public void ExtractPhysicalDriveNumber_Null_ReturnsNull()
    {
        Assert.Null(WmiDeviceDiscoveryService.ExtractPhysicalDriveNumber(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(@"\\.\HARDDISKVOLUME1")]
    [InlineData("PHYSICALDRIVE0")]
    [InlineData(@"\\.\PHYSICALDRIVE")]
    [InlineData(@"\\.\PHYSICALDRIVEABC")]
    public void ExtractPhysicalDriveNumber_InvalidId_ReturnsNull(string deviceId)
    {
        Assert.Null(WmiDeviceDiscoveryService.ExtractPhysicalDriveNumber(deviceId));
    }

    // === Group B: MapWmiMediaTypeString ===

    [Theory]
    [InlineData("Removable Media", MediaType.Flash)]
    [InlineData("removable media", MediaType.Flash)]
    [InlineData("External hard disk media", MediaType.HDD)]
    [InlineData("Fixed hard disk media", MediaType.HDD)]
    [InlineData("Solid State Drive", MediaType.SSD)]
    [InlineData("Something else", MediaType.Unknown)]
    [InlineData("", MediaType.Unknown)]
    public void MapWmiMediaTypeString_MapsCorrectly(string input, MediaType expected)
    {
        Assert.Equal(expected, WmiDeviceDiscoveryService.MapWmiMediaTypeString(input));
    }

    [Fact]
    public void MapWmiMediaTypeString_Null_ReturnsUnknown()
    {
        Assert.Equal(MediaType.Unknown, WmiDeviceDiscoveryService.MapWmiMediaTypeString(null));
    }

    // === Group C: MapMsftMediaTypeCode ===

    [Theory]
    [InlineData((ushort)0, MediaType.Unknown)]
    [InlineData((ushort)3, MediaType.HDD)]
    [InlineData((ushort)4, MediaType.SSD)]
    [InlineData((ushort)5, MediaType.SCM)]
    [InlineData((ushort)99, MediaType.Unknown)]
    public void MapMsftMediaTypeCode_MapsCorrectly(ushort code, MediaType expected)
    {
        Assert.Equal(expected, WmiDeviceDiscoveryService.MapMsftMediaTypeCode(code));
    }
}