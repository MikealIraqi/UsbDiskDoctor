using System;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Recovery.FakeCapacity;

namespace UsbDiskDoctor.App.Tests.TestDoubles;

public sealed class StubFakeCapacityChecker : IFakeCapacityChecker
{
    private readonly FakeCapacityResult _resultToReturn;
    private readonly TimeSpan _delay;
    private readonly Exception? _exceptionToThrow;

    public StubFakeCapacityChecker(
        FakeCapacityResult resultToReturn,
        TimeSpan delay = default,
        Exception? exceptionToThrow = null)
    {
        _resultToReturn = resultToReturn;
        _delay = delay;
        _exceptionToThrow = exceptionToThrow;
    }

    public int CallCount { get; private set; }

    public async Task<FakeCapacityResult> CheckAsync(
        FakeCapacityRequest request,
        IProgress<FakeCapacityProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;

        if (_delay > TimeSpan.Zero)
        {
            await Task.Delay(_delay, cancellationToken);
        }

        if (_exceptionToThrow is not null)
        {
            throw _exceptionToThrow;
        }

        progress?.Report(new FakeCapacityProgress
        {
            Phase = FakeCapacityPhase.Writing,
            BytesProcessed = 50,
            TotalBytes = 100,
            CurrentFileIndex = 1,
            TotalFiles = 2,
            Elapsed = TimeSpan.FromSeconds(1)
        });

        progress?.Report(new FakeCapacityProgress
        {
            Phase = FakeCapacityPhase.Reading,
            BytesProcessed = 100,
            TotalBytes = 100,
            CurrentFileIndex = 2,
            TotalFiles = 2,
            Elapsed = TimeSpan.FromSeconds(2)
        });

        cancellationToken.ThrowIfCancellationRequested();

        return _resultToReturn;
    }
}