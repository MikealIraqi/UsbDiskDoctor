using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UsbDiskDoctor.Recovery.CapacityChecking;
using UsbDiskDoctor.Recovery.Models;

namespace UsbDiskDoctor.App.ViewModels;

/// <summary>
/// ViewModel for capacity check operations, wrapping IFakeCapacityChecker for UI interaction.
/// </summary>
public sealed partial class CapacityCheckViewModel : ObservableObject
{
    private readonly IFakeCapacityChecker _checker;
    private CancellationTokenSource? _cts;

    public string DriveRoot { get; }
    public string TestFilePath { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCheckCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCheckCommand))]
    private bool _isRunning = false;

    [ObservableProperty]
    private CapacityCheckMode _selectedMode = CapacityCheckMode.Smart;

    [ObservableProperty]
    private bool _allowWriteOnNonEmpty = false;

    [ObservableProperty]
    private double _percentComplete = 0.0;

    [ObservableProperty]
    private string _currentPhaseText = "";

    [ObservableProperty]
    private string _speedText = "";

    [ObservableProperty]
    private string _etaText = "";

    [ObservableProperty]
    private string _elapsedText = "";

    [ObservableProperty]
    private CapacityVerdict _verdict = CapacityVerdict.Unknown;

    [ObservableProperty]
    private string _verdictText = "";

    [ObservableProperty]
    private string _verdictColorHex = "#A1A1AA";

    [ObservableProperty]
    private string _diagnosticMessage = "";

    [ObservableProperty]
    private bool _hasResult = false;

    public CapacityCheckViewModel(IFakeCapacityChecker checker, string driveRoot)
    {
        ArgumentNullException.ThrowIfNull(checker);
        ArgumentException.ThrowIfNullOrEmpty(driveRoot);

        _checker = checker;
        DriveRoot = driveRoot;
        TestFilePath = Path.Combine(driveRoot, "usbdiskdoctor_capacity_test.tmp");
        CurrentPhaseText = "جاهز";
        VerdictText = "لم يتم الفحص بعد";
        VerdictColorHex = "#A1A1AA";
    }

    private bool CanStartCheck() => !IsRunning;
    private bool CanCancelCheck() => IsRunning;

    [RelayCommand(CanExecute = nameof(CanStartCheck))]
    private async Task StartCheckAsync()
    {
        if (IsRunning) return;

        IsRunning = true;
        HasResult = false;
        PercentComplete = 0;

        _cts = new CancellationTokenSource();

        var options = new CapacityCheckOptions
        {
            Mode = SelectedMode,
            TestFilePath = TestFilePath,
            AllowWriteOnNonEmptyVolume = AllowWriteOnNonEmpty
        };

        var progress = new Progress<CapacityCheckProgress>(ApplyProgress);

        try
        {
            var result = await _checker.CheckAsync(DriveRoot, options, progress, _cts.Token);
            ApplyResult(result);
        }
        catch (OperationCanceledException)
        {
            CurrentPhaseText = "أُلغي الفحص";
            throw;
        }
        catch (Exception ex)
        {
            Verdict = CapacityVerdict.Failed;
            VerdictText = "فشل الفحص";
            VerdictColorHex = "#EF4444";
            DiagnosticMessage = ex.Message;
            HasResult = true;
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelCheck))]
    private void CancelCheck()
    {
        _cts?.Cancel();
    }

    private void ApplyProgress(CapacityCheckProgress progress)
    {
        PercentComplete = progress.PercentComplete;
        CurrentPhaseText = MapPhaseToArabic(progress.Phase);
        SpeedText = $"{progress.CurrentSpeedMbPerSec:F1} MB/s";
        EtaText = FormatTimeSpan(progress.EstimatedRemaining);
        ElapsedText = FormatTimeSpan(progress.Elapsed);
    }

    private void ApplyResult(CapacityCheckResult result)
    {
        Verdict = result.Verdict;
        VerdictText = MapVerdictToArabic(result.Verdict);
        VerdictColorHex = MapVerdictToColorHex(result.Verdict);
        DiagnosticMessage = result.DiagnosticMessage ?? "";
        PercentComplete = 100.0;
        HasResult = true;
    }

    internal static string MapPhaseToArabic(CapacityCheckPhase phase) =>
        phase switch
        {
            CapacityCheckPhase.Idle => "خامل",
            CapacityCheckPhase.Preparing => "جاري التحضير...",
            CapacityCheckPhase.Writing => "جاري الكتابة...",
            CapacityCheckPhase.Reading => "جاري القراءة...",
            CapacityCheckPhase.Verifying => "جاري التحقق...",
            CapacityCheckPhase.Completed => "اكتمل",
            CapacityCheckPhase.Failed => "فشل",
            CapacityCheckPhase.Cancelled => "أُلغي",
            _ => "غير معروف"
        };

    internal static string MapVerdictToArabic(CapacityVerdict verdict) =>
        verdict switch
        {
            CapacityVerdict.Unknown => "لم يتم الفحص بعد",
            CapacityVerdict.Genuine => "السعة حقيقية ✓",
            CapacityVerdict.Fake => "السعة مزيفة ✗",
            CapacityVerdict.Inconclusive => "نتيجة غير حاسمة",
            CapacityVerdict.Failed => "فشل الفحص",
            _ => "غير معروف"
        };

    internal static string MapVerdictToColorHex(CapacityVerdict verdict) =>
        verdict switch
        {
            CapacityVerdict.Genuine => "#10B981",
            CapacityVerdict.Fake => "#EF4444",
            CapacityVerdict.Inconclusive => "#F59E0B",
            CapacityVerdict.Failed => "#EF4444",
            _ => "#A1A1AA"
        };

    internal static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts <= TimeSpan.Zero) return "—";
        if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}س {ts.Minutes}د";
        if (ts.TotalMinutes >= 1) return $"{(int)ts.TotalMinutes}د {ts.Seconds}ث";
        return $"{(int)ts.TotalSeconds}ث";
    }
}