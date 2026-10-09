using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UsbDiskDoctor.App.Tests.TestDoubles;
using UsbDiskDoctor.App.ViewModels;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Knowledge.Services;
using UsbDiskDoctor.Recovery.FakeCapacity;
using Xunit;

namespace UsbDiskDoctor.App.Tests.ViewModels;

public sealed class FakeCapacityViewModelTests
{
    private sealed class StubKnowledgeService : IKnowledgeService
    {
        public int Load() => 0;
        public KnowledgeArticle? GetArticle(string code) => null;
        public IReadOnlyList<KnowledgeArticle> GetAllArticles() => Array.Empty<KnowledgeArticle>();
        public bool IsLoaded => true;
        public int ArticleCount => 0;
    }

    private sealed class StubKnowledgeServiceWithArticle : IKnowledgeService
    {
        private readonly KnowledgeArticle _article;

        public StubKnowledgeServiceWithArticle(KnowledgeArticle article)
        {
            _article = article;
        }

        public int Load() => 1;

        public KnowledgeArticle? GetArticle(string code)
        {
            return code == "FAKE_CAPACITY_DETECTED" ? _article : null;
        }

        public IReadOnlyList<KnowledgeArticle> GetAllArticles() => new[] { _article };
        public bool IsLoaded => true;
        public int ArticleCount => 1;
    }

    private static FakeCapacityResult MakeResult(FakeCapacityStatus status) => new()
    {
        Status = status,
        TotalBytesWritten = 100,
        TotalBytesRead = 100,
        FailedBytes = 0,
        FailurePercentage = 0,
        DetectedRealCapacityBytes = 100,
        Details = "test",
        Duration = TimeSpan.FromSeconds(1)
    };

    [Fact]
    public void Constructor_WithNullChecker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FakeCapacityViewModel(null!, new StubKnowledgeService(), 'Z'));
    }

    [Fact]
    public void Constructor_WithNullKnowledgeService_ThrowsArgumentNullException()
    {
        var checker = new StubFakeCapacityChecker(MakeResult(FakeCapacityStatus.Genuine));
        Assert.Throws<ArgumentNullException>(() =>
            new FakeCapacityViewModel(checker, null!, 'Z'));
    }

    [Fact]
    public void Constructor_WithLowercaseLetter_UppercasesDriveLetter()
    {
        var checker = new StubFakeCapacityChecker(MakeResult(FakeCapacityStatus.Genuine));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'z');
        Assert.Equal('Z', vm.DriveLetter);
    }

    [Fact]
    public void StartCommand_InitialState_CanExecuteTrue()
    {
        var checker = new StubFakeCapacityChecker(MakeResult(FakeCapacityStatus.Genuine));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'Z');
        Assert.True(vm.CanStart);
        Assert.True(vm.StartCommand.CanExecute(null));
    }

    [Fact]
    public async Task StartCommand_AfterStart_CanExecuteFalse_DuringExecution()
    {
        var checker = new StubFakeCapacityChecker(
            MakeResult(FakeCapacityStatus.Genuine),
            delay: TimeSpan.FromMilliseconds(200));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'Z');

        var task = vm.StartCommand.ExecuteAsync(null);

        await Task.Delay(50);

        Assert.False(vm.CanStart);
        Assert.False(vm.StartCommand.CanExecute(null));
        Assert.True(vm.CanCancel);
        Assert.True(vm.CancelCommand.CanExecute(null));

        await task;
    }

    [Fact]
    public async Task StartCommand_WithGenuineResult_SetsGreenStatusAndEmptyActions()
    {
        var checker = new StubFakeCapacityChecker(MakeResult(FakeCapacityStatus.Genuine));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'Z');

        await vm.StartCommand.ExecuteAsync(null);

        Assert.Equal("#10B981", vm.StatusColorHex);
        Assert.Empty(vm.SafeActions);
    }

    [Fact]
    public async Task StartCommand_WithFakeCapacityResult_SetsRedStatusAndLoadsArticle()
    {
        var article = new KnowledgeArticle
        {
            Code = "FAKE_CAPACITY_DETECTED",
            TitleAr = "قرص مزيف",
            SafeActions = new List<string> { "إجراء آمن 1", "إجراء آمن 2" },
            DangerousActions = new List<string> { "إجراء خطر" }
        };

        var knowledgeService = new StubKnowledgeServiceWithArticle(article);
        var checker = new StubFakeCapacityChecker(MakeResult(FakeCapacityStatus.FakeCapacity));
        var vm = new FakeCapacityViewModel(checker, knowledgeService, 'Z');

        await vm.StartCommand.ExecuteAsync(null);

        Assert.Equal("#EF4444", vm.StatusColorHex);
        Assert.True(vm.SafeActions.Count > 0);
        Assert.True(vm.DangerousActions.Count > 0);
    }

    [Fact]
    public async Task StartCommand_WithError_ThrowsException_SetsErrorMessage()
    {
        var checker = new StubFakeCapacityChecker(
            MakeResult(FakeCapacityStatus.Genuine),
            exceptionToThrow: new InvalidOperationException("boom"));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'Z');

        await vm.StartCommand.ExecuteAsync(null);

        Assert.Contains("boom", vm.ErrorMessage);
        Assert.Equal("#EF4444", vm.StatusColorHex);
    }

    [Fact]
    public async Task StartCommand_WithCancellation_SetsCancelledStatus()
    {
        var checker = new StubFakeCapacityChecker(
            MakeResult(FakeCapacityStatus.Genuine),
            delay: TimeSpan.FromMilliseconds(500));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'Z');

        var task = vm.StartCommand.ExecuteAsync(null);

        await Task.Delay(100);
        vm.CancelCommand.Execute(null);

        await task;

        Assert.Equal("تم الإلغاء", vm.StatusText);
    }

    [Fact]
    public async Task ProgressReporting_UpdatesProgressPercent()
    {
        var checker = new StubFakeCapacityChecker(MakeResult(FakeCapacityStatus.Genuine));
        var vm = new FakeCapacityViewModel(checker, new StubKnowledgeService(), 'Z');

        await vm.StartCommand.ExecuteAsync(null);

        Assert.Equal(100.0, vm.ProgressPercent);
    }
}