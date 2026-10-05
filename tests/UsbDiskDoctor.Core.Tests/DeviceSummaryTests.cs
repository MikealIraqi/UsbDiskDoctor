using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using Xunit;

namespace UsbDiskDoctor.Core.Tests
{
    public class DeviceSummaryTests
    {
        [Fact]
        public void NewDeviceSummary_HasSafeDefaults()
        {
            var summary = new DeviceSummary();

            Assert.Equal(string.Empty, summary.DeviceId);
            Assert.Equal(string.Empty, summary.FriendlyName);
            Assert.NotNull(summary.Volumes);
            Assert.Empty(summary.Volumes);
            Assert.Equal(BusType.Unknown, summary.BusType);
            Assert.Equal(MediaType.Unknown, summary.MediaType);
            Assert.Equal(PartitionStyle.Unknown, summary.PartitionStyle);
            Assert.Equal(0, summary.SizeBytes);
            Assert.Equal(HealthStatus.Unknown, summary.HealthStatus);
            Assert.Equal(OperationalStatus.Unknown, summary.OperationalStatus);
            Assert.False(summary.IsUsb);
            Assert.False(summary.IsExternal);
        }

        [Fact]
        public void WithExpression_OverridesSingleProperty()
        {
            var original = new DeviceSummary
            {
                DeviceId = "\\\\.\\PHYSICALDRIVE1",
                FriendlyName = "Original Device"
            };

            var modified = original with { FriendlyName = "Modified Device" };

            Assert.Equal("Modified Device", modified.FriendlyName);
            Assert.Equal("Original Device", original.FriendlyName);
            Assert.Equal("\\\\.\\PHYSICALDRIVE1", modified.DeviceId);
        }
    }
}