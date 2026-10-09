using System;
using System.Collections.Generic;
using UsbDiskDoctor.Recovery.FakeCapacity;

namespace UsbDiskDoctor.Recovery.Tests.TestDoubles;

public sealed class FakeDriveInfoProvider : IDriveInfoProvider
{
    private readonly Dictionary<char, DriveInfoEx> _drives;

    public FakeDriveInfoProvider(IEnumerable<DriveInfoEx> drives)
    {
        _drives = new Dictionary<char, DriveInfoEx>();
        foreach (var drive in drives)
        {
            var key = char.ToUpperInvariant(drive.DriveLetter);
            _drives[key] = drive;
        }
    }

    public DriveInfoEx? GetDrive(char driveLetter)
    {
        var key = char.ToUpperInvariant(driveLetter);
        return _drives.TryGetValue(key, out var drive) ? drive : null;
    }

    public IEnumerable<DriveInfoEx> GetAllDrives() => _drives.Values;
}