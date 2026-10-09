using System;
using System.Security.Cryptography;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public static class PatternGenerator
{
    public static byte[] Generate(int fileIndex, int sizeBytes)
    {
        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "Size must be greater than zero");
        }

        var seed = fileIndex * 7919 + 1;
        var rng = new Random(seed);
        var data = new byte[sizeBytes];
        rng.NextBytes(data);
        return data;
    }

    public static string ComputeSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToHexString(hash);
    }

    public static bool IsValidFileSize(int size)
    {
        return size > 0 && size % 4096 == 0;
    }
}