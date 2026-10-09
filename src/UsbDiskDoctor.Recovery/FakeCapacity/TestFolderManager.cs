using System;
using System.IO;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public sealed class TestFolderManager : IDisposable
{
    private readonly string _folderPath;
    private bool _disposed;

    public TestFolderManager(char driveLetter)
    {
        var upperLetter = char.ToUpperInvariant(driveLetter);
        var guid = Guid.NewGuid().ToString("N");
        _folderPath = $"{upperLetter}:\\_UsbDiskDoctor_Test_{guid}";

        if (Directory.Exists(_folderPath))
        {
            throw new InvalidOperationException($"Test folder already exists: {_folderPath}");
        }

        Directory.CreateDirectory(_folderPath);
    }

    public string FolderPath => _folderPath;

    public string GetFilePath(int index)
    {
        return Path.Combine(_folderPath, $"pattern_{index:D4}.bin");
    }

    public void Cleanup()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (Directory.Exists(_folderPath))
            {
                Directory.Delete(_folderPath, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Cleanup();
    }
}