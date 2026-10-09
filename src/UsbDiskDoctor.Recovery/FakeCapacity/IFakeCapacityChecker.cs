using System;
using System.Threading;
using System.Threading.Tasks;

namespace UsbDiskDoctor.Recovery.FakeCapacity;

public interface IFakeCapacityChecker
{
    Task<FakeCapacityResult> CheckAsync(
        FakeCapacityRequest request,
        IProgress<FakeCapacityProgress>? progress = null,
        CancellationToken cancellationToken = default);
}