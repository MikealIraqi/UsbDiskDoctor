using System;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public static class FakeCapacitySafetyPolicy
{
    public static SafetyCheckResult Validate(DriveInfoEx drive)
    {
        if (drive.DriveLetter == '\0')
        {
            return Fail("Drive letter is not set");
        }

        char upperLetter = char.ToUpperInvariant(drive.DriveLetter);
        if (upperLetter < 'A' || upperLetter > 'Z')
        {
            return Fail("Drive letter is not a valid letter (A-Z)");
        }

        string systemDir = Environment.SystemDirectory;
        if (systemDir.Length > 0)
        {
            char systemDriveLetter = char.ToUpperInvariant(systemDir[0]);
            if (systemDriveLetter == upperLetter)
            {
                return Fail("Cannot test the system drive");
            }
        }

        if (!string.Equals(drive.BusType, "USB", StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Drive is not a USB device");
        }

        if (drive.TotalSizeBytes <= 0)
        {
            return Fail("Drive size is invalid");
        }

        return new SafetyCheckResult
        {
            IsSafe = true,
            Reason = "OK"
        };
    }

    private static SafetyCheckResult Fail(string reason) => new()
    {
        IsSafe = false,
        Reason = reason
    };
}