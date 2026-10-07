using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace UsbDiskDoctor.App.Localization;

/// <summary>
/// Runtime language switching service. Swaps ResourceDictionary between
/// Arabic and English at runtime without restarting the application.
/// </summary>
public static class LocalizationService
{
    private const string RegistryKeyPath = @"Software\UsbDiskDoctor";
    private const string RegistryValueName = "Language";
    private const string ArDictRelative = "Localization/Strings.ar.xaml";
    private const string EnDictRelative = "Localization/Strings.en.xaml";
    private static ResourceDictionary? _currentStringsDict;

    /// <summary>Raised after the language dictionary has been swapped.</summary>
    public static event Action? LanguageChanged;

    /// <summary>Current language code: "ar" or "en".</summary>
    public static string CurrentLanguage { get; private set; } = "ar";

    /// <summary>True when current language is Arabic (RTL).</summary>
    public static bool IsArabic => CurrentLanguage == "ar";

    /// <summary>Flow direction matching the current language.</summary>
    public static FlowDirection CurrentFlowDirection
        => IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>
    /// Initializes the service by loading the saved language (or Arabic default).
    /// Call this once during application startup.
    /// </summary>
    public static void Initialize()
    {
        var saved = LoadSavedLanguage();
        ApplyLanguage(saved, raiseEvent: false);
    }

    /// <summary>
    /// Switches to the given language ("ar" or "en") and persists the choice.
    /// Raises LanguageChanged so views can update FlowDirection.
    /// </summary>
    public static void SetLanguage(string lang)
    {
        if (lang != "ar" && lang != "en")
        {
            throw new ArgumentException($"Unsupported language: {lang}", nameof(lang));
        }
        if (lang == CurrentLanguage)
        {
            return;
        }
        ApplyLanguage(lang, raiseEvent: true);
        SaveLanguage(lang);
    }

    /// <summary>Toggles between Arabic and English.</summary>
    public static void ToggleLanguage()
    {
        SetLanguage(IsArabic ? "en" : "ar");
    }

    /// <summary>
    /// Looks up a localized string by key. Returns the key itself if not found
    /// (fail-soft, never throws).
    /// </summary>
    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }
        var value = Application.Current?.TryFindResource(key);
        return value as string ?? key;
    }

    private static void ApplyLanguage(string lang, bool raiseEvent)
    {
        if (Application.Current is null)
        {
            return;
        }

        var dicts = Application.Current.Resources.MergedDictionaries;

        if (_currentStringsDict is not null)
        {
            dicts.Remove(_currentStringsDict);
            _currentStringsDict = null;
        }

        for (int i = dicts.Count - 1; i >= 0; i--)
        {
            var src = dicts[i].Source?.OriginalString ?? string.Empty;
            if (src.IndexOf("Strings.", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                dicts.RemoveAt(i);
            }
        }

        var relative = lang == "ar" ? ArDictRelative : EnDictRelative;
        var newDict = new ResourceDictionary
        {
            Source = new Uri(relative, UriKind.Relative)
        };
        dicts.Add(newDict);
        _currentStringsDict = newDict;

        CurrentLanguage = lang;
        CultureInfo.CurrentUICulture = new CultureInfo(lang);

        if (raiseEvent)
        {
            LanguageChanged?.Invoke();
        }
    }

    private static string LoadSavedLanguage()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            var value = key?.GetValue(RegistryValueName) as string;
            if (value == "ar" || value == "en")
            {
                return value;
            }
        }
        catch
        {
            // Ignore registry errors - default to Arabic.
        }
        return "ar";
    }

    private static void SaveLanguage(string lang)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            key?.SetValue(RegistryValueName, lang);
        }
        catch
        {
            // Ignore registry errors - language still works for this session.
        }
    }
}
