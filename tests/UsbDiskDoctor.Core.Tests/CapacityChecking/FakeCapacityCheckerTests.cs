// File: tests/UsbDiskDoctor.Core.Tests/CapacityChecking/FakeCapacityCheckerTests.cs
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Recovery.CapacityChecking;
using UsbDiskDoctor.Recovery.Models;
using Xunit;

namespace UsbDiskDoctor.Core.Tests.CapacityChecking;

public sealed class FakeCapacityCheckerTests
{
    private static CapacityCheckOptions ValidOptions() => new()
    {
        TestFilePath = "C:\\temp\\does-not-matter.tmp"
    };

    // === Group A: Guards ===

    [Fact]
    public async Task CheckAsync_NullDriveRoot_ThrowsArgumentNullException()
    {
        var checker = new FakeCapacityChecker();
        var options = ValidOptions();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            checker.CheckAsync(null!, options, null, CancellationToken.None));
    }

    [Fact]
    public async Task CheckAsync_EmptyDriveRoot_ThrowsArgumentException()
    {
        var checker = new FakeCapacityChecker();
        var options = ValidOptions();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            checker.CheckAsync("", options, null, CancellationToken.None));
    }

    [Fact]
    public async Task CheckAsync_NullOptions_ThrowsArgumentNullException()
    {
        var checker = new FakeCapacityChecker();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            checker.CheckAsync("C:\\", null!, null, CancellationToken.None));
    }

    [Fact]
    public async Task CheckAsync_EmptyTestFilePath_ThrowsArgumentException()
    {
        var checker = new FakeCapacityChecker();
        var options = ValidOptions() with { TestFilePath = "" };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            checker.CheckAsync("C:\\", options, null, CancellationToken.None));
    }

    [Fact]
    public async Task CheckAsync_BlockSizeTooSmall_ThrowsArgumentException()
    {
        var checker = new FakeCapacityChecker();
        var options = ValidOptions() with { BlockSizeBytes = 16 };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            checker.CheckAsync("C:\\", options, null, CancellationToken.None));
    }

    // === Group B: Path validation ===

    [Fact]
    public async Task CheckAsync_TestFilePathOnDifferentVolume_ReturnsFailed()
    {
        var checker = new FakeCapacityChecker();
        var options = ValidOptions() with { TestFilePath = "Z:\\nonexistent.tmp" };

        var result = await checker.CheckAsync("C:\\", options, null, CancellationToken.None);

        Assert.Equal(CapacityVerdict.Failed, result.Verdict);
        Assert.Contains("same volume", result.DiagnosticMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckAsync_InvalidDriveRoot_ReturnsFailed()
    {
        var checker = new FakeCapacityChecker();
        var options = ValidOptions() with { TestFilePath = "Z:\\test.tmp" };

        var result = await checker.CheckAsync("Z:\\", options, null, CancellationToken.None);

        Assert.Equal(CapacityVerdict.Failed, result.Verdict);
    }

    // === Group C: ComputeSampleIndices ===

    [Fact]
    public void ComputeSampleIndices_Quick100_Returns100DistinctIndices()
    {
        var result = FakeCapacityChecker.ComputeSampleIndices(128_000L, 100);

        Assert.Equal(100, result.Count);
        Assert.Equal(100, result.Distinct().Count());
    }

    [Fact]
    public void ComputeSampleIndices_SingleSample_ReturnsZero()
    {
        var result = FakeCapacityChecker.ComputeSampleIndices(128_000L, 1);

        Assert.Single(result);
        Assert.Equal(0L, result[0]);
    }

    [Fact]
    public void ComputeSampleIndices_FirstAndLastIncluded()
    {
        var result = FakeCapacityChecker.ComputeSampleIndices(128_000L, 100);

        Assert.Contains(0L, result);
        Assert.Contains(127_999L, result);
    }

    [Fact]
    public void ComputeSampleIndices_ResultIsSortedAscending()
    {
        var result = FakeCapacityChecker.ComputeSampleIndices(128_000L, 50);

        Assert.Equal(result.OrderBy(x => x), result);
    }

    [Fact]
    public void ComputeSampleIndices_SmallDriveThanSampleCount_DedupesToAtMostTotalBlocks()
    {
        var result = FakeCapacityChecker.ComputeSampleIndices(10L, 100);

        Assert.True(result.Count <= 10);
    }

    // === Group D: ComputeActualCapacity ===

    [Fact]
    public void ComputeActualCapacity_NoWraparound_ReturnsFullClaimed()
    {
        var result = FakeCapacityChecker.ComputeActualCapacity(128_000L, 1_048_576L, 500L, 500L);

        Assert.Equal(128_000L * 1_048_576L, result);
    }

    [Fact]
    public void ComputeActualCapacity_TypicalFakeFlash_ReturnsActualSize()
    {
        var result = FakeCapacityChecker.ComputeActualCapacity(128_000L, 1_048_576L, 0L, 127_872L);

        Assert.Equal(128L * 1_048_576L, result);
    }

    [Fact]
    public void ComputeActualCapacity_ClampsToOneBlock_WhenDeltaExceedsTotal()
    {
        var result = FakeCapacityChecker.ComputeActualCapacity(10L, 1_048_576L, 0L, 1_000L);

        Assert.Equal(1_048_576L, result);
    }

    [Fact]
    public void ComputeActualCapacity_HandlesLargeNumbersWithoutOverflow()
    {
        var result = FakeCapacityChecker.ComputeActualCapacity(10_000_000L, 512L, 0L, 0L);

        Assert.Equal(10_000_000L * 512L, result);
    }

    // === Group E: BuildBlock + ParseBlockIndex ===

    [Fact]
    public void BuildBlock_WritesMagicHeaderUSBDDOC1()
    {
        var buffer = new byte[1024];
        FakeCapacityChecker.BuildBlock(buffer, 42L, 1024);

        Assert.Equal(0x55, buffer[0]);
        Assert.Equal(0x53, buffer[1]);
        Assert.Equal(0x42, buffer[2]);
        Assert.Equal(0x44, buffer[3]);
        Assert.Equal(0x44, buffer[4]);
        Assert.Equal(0x4F, buffer[5]);
        Assert.Equal(0x43, buffer[6]);
        Assert.Equal(0x31, buffer[7]);
    }

    [Fact]
    public void BuildBlock_ClearsPaddingAfterHeader()
    {
        var buffer = new byte[1024];
        Array.Fill(buffer, (byte)0xFF);
        FakeCapacityChecker.BuildBlock(buffer, 42L, 1024);

        for (int i = 16; i < 1024; i++)
        {
            Assert.Equal(0, buffer[i]);
        }
    }

    [Fact]
    public void ParseBlockIndex_RoundTripPreservesIndex()
    {
        var buffer = new byte[1024];
        FakeCapacityChecker.BuildBlock(buffer, 12345L, 1024);

        Assert.Equal(12345L, FakeCapacityChecker.ParseBlockIndex(buffer));
    }

    [Fact]
    public void ParseBlockIndex_MaxInt64Index_RoundTrip()
    {
        var buffer = new byte[1024];
        FakeCapacityChecker.BuildBlock(buffer, long.MaxValue, 1024);

        Assert.Equal(long.MaxValue, FakeCapacityChecker.ParseBlockIndex(buffer));
    }
}