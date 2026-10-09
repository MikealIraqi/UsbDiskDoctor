using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UsbDiskDoctor.Knowledge.Services;
using UsbDiskDoctor.Recovery.FakeCapacity;

namespace UsbDiskDoctor.App.ViewModels;

public sealed partial class FakeCapacityViewModel : ObservableObject
{
    private readonly IFakeCapacityChecker _checker;
    private readonly IKnowledgeService _knowledgeService;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private char _driveLetter;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    private bool _isBusy;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _progressText = "";

    [ObservableProperty]
    private string _statusText = "لم يبدأ الفحص بعد";

    [ObservableProperty]
    private string _statusColorHex = "#71717A";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    private FakeCapacityResult? _result;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public FakeCapacityViewModel(
        IFakeCapacityChecker checker,
        IKnowledgeService knowledgeService,
        char driveLetter)
    {
        _checker = checker ?? throw new ArgumentNullException(nameof(checker));
        _knowledgeService = knowledgeService ?? throw new ArgumentNullException(nameof(knowledgeService));
        DriveLetter = char.ToUpperInvariant(driveLetter);
        SafeActions = new ObservableCollection<string>();
        DangerousActions = new ObservableCollection<string>();
    }

    public string DriveLetterDisplay => $"{DriveLetter}:";
    public bool CanStart => !IsBusy;
    public bool CanCancel => IsBusy;
    public bool HasResult => Result is not null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ObservableCollection<string> SafeActions { get; }
    public ObservableCollection<string> DangerousActions { get; }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync(CancellationToken externalToken)
    {
        IsBusy = true;
        ProgressPercent = 0;
        ProgressText = "جاري التحضير...";
        StatusText = "جاري الفحص...";
        StatusColorHex = "#4F46E5";
        Result = null;
        ErrorMessage = null;
        SafeActions.Clear();
        DangerousActions.Clear();

        _cts = new CancellationTokenSource();
        try
        {
            var request = new FakeCapacityRequest
            {
                DriveLetter = DriveLetter,
                QuickTest = true
            };

            var progress = new Progress<FakeCapacityProgress>(OnProgress);
            var result = await _checker.CheckAsync(request, progress, _cts.Token);

            Result = result;
            ApplyResult(result);
        }
        catch (OperationCanceledException)
        {
            StatusText = "تم الإلغاء";
            StatusColorHex = "#71717A";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            StatusText = "فشل الفحص";
            StatusColorHex = "#EF4444";
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            IsBusy = false;
        }
    }

    private void OnProgress(FakeCapacityProgress p)
    {
        double percent = p.TotalBytes > 0
            ? (double)p.BytesProcessed / p.TotalBytes * 100.0
            : 0.0;

        if (percent < 0) percent = 0;
        if (percent > 100) percent = 100;

        ProgressPercent = percent;
        ProgressText = TranslatePhase(p.Phase);
    }

    private static string TranslatePhase(FakeCapacityPhase phase) => phase switch
    {
        FakeCapacityPhase.Writing => "جاري الكتابة...",
        FakeCapacityPhase.Flushing => "جاري التثبيت على القرص...",
        FakeCapacityPhase.Reading => "جاري القراءة والتحقق...",
        FakeCapacityPhase.Verifying => "جاري التحقق من البصمات...",
        FakeCapacityPhase.Completed => "اكتمل",
        _ => ""
    };

    private void ApplyResult(FakeCapacityResult result)
    {
        switch (result.Status)
        {
            case FakeCapacityStatus.Genuine:
                StatusText = "القرص سليم — السعة حقيقية";
                StatusColorHex = "#10B981";
                ProgressPercent = 100;
                ProgressText = "اكتمل";
                break;

            case FakeCapacityStatus.Suspect:
                StatusText = "تحذير — يشتبه في السعة";
                StatusColorHex = "#F59E0B";
                LoadKnowledgeArticle();
                break;

            case FakeCapacityStatus.FakeCapacity:
                StatusText = "قرص مزيف السعة!";
                StatusColorHex = "#EF4444";
                LoadKnowledgeArticle();
                break;

            case FakeCapacityStatus.Inconclusive:
                StatusText = "نتيجة غير حاسمة — أعد المحاولة";
                StatusColorHex = "#71717A";
                break;

            case FakeCapacityStatus.Error:
                StatusText = "فشل الفحص";
                StatusColorHex = "#EF4444";
                ErrorMessage = result.Details;
                break;
        }
    }

    private void LoadKnowledgeArticle()
    {
        try
        {
            var article = _knowledgeService.GetArticle("FAKE_CAPACITY_DETECTED");
            if (article is null) return;

            SafeActions.Clear();
            foreach (var action in article.SafeActions)
            {
                SafeActions.Add(action);
            }

            DangerousActions.Clear();
            foreach (var action in article.DangerousActions)
            {
                DangerousActions.Add(action);
            }
        }
        catch
        {
            // Fail-soft
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // تم dispose مسبقاً
        }
    }
}