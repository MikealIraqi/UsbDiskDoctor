// File: tests/UsbDiskDoctor.App.Tests/CapacityChecking/CapacityCheckViewModelTests.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.Recovery.CapacityChecking;
using UsbDiskDoctor.Recovery.Models;
using Xunit;

namespace UsbDiskDoctor.App.Tests.CapacityChecking;

internal sealed class FakeCapacityCheckerStub : IFakeCapacityChecker
{
    public CapacityCheckResult? ResultToReturn { get; set; }
    public Exception? ExceptionToThrow { get; set; }
    public TaskCompletionSource? GateTcs { get; set; }
    public bool WasCalled { get; private set; }
    public CapacityCheckOptions? LastOptions { get; private set; }

    public async Task<CapacityCheckResult> CheckAsync(
        string driveRoot,
        CapacityCheckOptions options,
        IProgress<CapacityCheckProgress>? progress,
        CancellationToken cancellationToken)
    {
        WasCalled = true;
        LastOptions = options;

        if (GateTcs is not null)
        {
            await GateTcs.Task;
        }

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return ResultToReturn ?? new CapacityCheckResult
        {
            DriveRoot = driveRoot,
            Verdict = CapacityVerdict.Genuine
        };
    }
}

public sealed class CapacityCheckViewModelTests
{
    [Fact]
    public void Constructor_NullChecker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CapacityCheckViewModel(null!, "C:\\"));
    }

    [Fact]
    public void Constructor_EmptyDriveRoot_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CapacityCheckViewModel(new FakeCapacityCheckerStub(), ""));
    }

    [Fact]
    public void Constructor_SetsDriveRootAndTestFilePath()
    {
        var vm = new CapacityCheckViewModel(new FakeCapacityCheckerStub(), "C:\\");

        Assert.Equal("C:\\", vm.DriveRoot);
        Assert.Contains("usbdiskdoctor_capacity_test", vm.TestFilePath);
    }

    [Fact]
    public void SelectedMode_DefaultsToSmart()
    {
        var vm = new CapacityCheckViewModel(new FakeCapacityCheckerStub(), "C:\\");

        Assert.Equal(CapacityCheckMode.Smart, vm.SelectedMode);
    }

    [Fact]
    public void StartCheckCommand_CanExecute_TrueWhenIdle()
    {
        var vm = new CapacityCheckViewModel(new FakeCapacityCheckerStub(), "C:\\");

        Assert.True(vm.StartCheckCommand.CanExecute(null));
    }

    [Fact]
    public async Task StartCheckCommand_CanExecute_FalseWhenRunning()
    {
        var stub = new FakeCapacityCheckerStub
        {
            GateTcs = new TaskCompletionSource()
        };
        var vm = new CapacityCheckViewModel(stub, "C:\\");

        var task = vm.StartCheckCommand.ExecuteAsync(null);

        Assert.True(vm.IsRunning);
        Assert.False(vm.StartCheckCommand.CanExecute(null));

        stub.GateTcs.SetResult();
        await task;
    }

    [Fact]
    public async Task CancelCommand_CanExecute_TrueOnlyWhenRunning()
    {
        var stub = new FakeCapacityCheckerStub
        {
            GateTcs = new TaskCompletionSource()
        };
        var vm = new CapacityCheckViewModel(stub, "C:\\");

        Assert.False(vm.CancelCheckCommand.CanExecute(null));

        var task = vm.StartCheckCommand.ExecuteAsync(null);

        Assert.True(vm.CancelCheckCommand.CanExecute(null));

        stub.GateTcs.SetResult();
        await task;
    }

    [Theory]
    [InlineData(CapacityVerdict.Unknown)]
    [InlineData(CapacityVerdict.Genuine)]
    [InlineData(CapacityVerdict.Fake)]
    [InlineData(CapacityVerdict.Inconclusive)]
    [InlineData(CapacityVerdict.Failed)]
    public void MapVerdictToArabic_AllVerdicts_HaveNonEmptyText(CapacityVerdict verdict)
    {
        var text = CapacityCheckViewModel.MapVerdictToArabic(verdict);

        Assert.False(string.IsNullOrWhiteSpace(text));
    }
}