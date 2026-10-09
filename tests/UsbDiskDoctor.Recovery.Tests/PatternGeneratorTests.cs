using System;
using System.Linq;
using UsbDiskDoctor.Recovery.FakeCapacity;
using Xunit;

namespace UsbDiskDoctor.Recovery.Tests;

public sealed class PatternGeneratorTests
{
    [Fact]
    public void Generate_WithSizeZero_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PatternGenerator.Generate(0, 0));
    }

    [Fact]
    public void Generate_WithNegativeSize_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PatternGenerator.Generate(0, -1));
    }

    [Fact]
    public void Generate_WithValidSize_ReturnsCorrectLength()
    {
        var result = PatternGenerator.Generate(0, 4096);
        Assert.Equal(4096, result.Length);
    }

    [Fact]
    public void Generate_WithSameIndex_ReturnsSameBytes()
    {
        var first = PatternGenerator.Generate(42, 4096);
        var second = PatternGenerator.Generate(42, 4096);
        Assert.True(first.SequenceEqual(second));
    }

    [Fact]
    public void Generate_WithDifferentIndex_ReturnsDifferentBytes()
    {
        var first = PatternGenerator.Generate(0, 4096);
        var second = PatternGenerator.Generate(1, 4096);
        Assert.False(first.SequenceEqual(second));
    }

    [Fact]
    public void ComputeSha256_WithEmptyArray_ReturnsKnownHash()
    {
        var hash = PatternGenerator.ComputeSha256(Array.Empty<byte>());
        Assert.Equal("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855", hash);
    }

    [Fact]
    public void ComputeSha256_WithSameData_ReturnsSameHash()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        var first = PatternGenerator.ComputeSha256(data);
        var second = PatternGenerator.ComputeSha256(data);
        Assert.Equal(first, second);
    }

    [Fact]
    public void ComputeSha256_WithDifferentData_ReturnsDifferentHash()
    {
        var first = PatternGenerator.ComputeSha256(new byte[] { 1, 2, 3 });
        var second = PatternGenerator.ComputeSha256(new byte[] { 4, 5, 6 });
        Assert.NotEqual(first, second);
    }

    [Theory]
    [InlineData(4096)]
    [InlineData(65536)]
    [InlineData(1048576)]
    public void IsValidFileSize_WithValidMultipleOf4096_ReturnsTrue(int size)
    {
        Assert.True(PatternGenerator.IsValidFileSize(size));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4095)]
    [InlineData(100)]
    public void IsValidFileSize_WithNonMultipleOf4096_ReturnsFalse(int size)
    {
        Assert.False(PatternGenerator.IsValidFileSize(size));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-4096)]
    public void IsValidFileSize_WithZeroOrNegative_ReturnsFalse(int size)
    {
        Assert.False(PatternGenerator.IsValidFileSize(size));
    }
}