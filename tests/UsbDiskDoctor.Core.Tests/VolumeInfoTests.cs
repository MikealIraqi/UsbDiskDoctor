using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using Xunit;

namespace UsbDiskDoctor.Core.Tests
{
    public class VolumeInfoTests
    {
        [Fact]
        public void NewVolumeInfo_HasSafeDefaults()
        {
            var volume = new VolumeInfo();

            Assert.Null(volume.DriveLetter);
            Assert.Equal(string.Empty, volume.Label);
            Assert.Equal(FileSystemType.Unknown, volume.FileSystem);
            Assert.Equal(0, volume.SizeBytes);
            Assert.Equal(0, volume.FreeSpaceBytes);
            Assert.Equal(HealthStatus.Unknown, volume.HealthStatus);
            Assert.False(volume.IsRaw);
            Assert.False(volume.IsMounted);
        }

        [Fact]
        public void IsRaw_FalseByDefault()
        {
            var volume = new VolumeInfo();

            Assert.False(volume.IsRaw);
        }
    }
}