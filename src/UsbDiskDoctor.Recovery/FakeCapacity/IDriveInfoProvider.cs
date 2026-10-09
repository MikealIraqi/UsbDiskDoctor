using System.Collections.Generic;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public interface IDriveInfoProvider
{
    DriveInfoEx? GetDrive(char driveLetter);
    IEnumerable<DriveInfoEx> GetAllDrives();
}