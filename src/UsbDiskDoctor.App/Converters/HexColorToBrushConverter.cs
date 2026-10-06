using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UsbDiskDoctor.App.Converters;

/// <summary>
/// Converts a hex color string (e.g. "#EF4444") to a frozen SolidColorBrush.
/// Returns a muted fallback brush if the input is null or invalid.
/// </summary>
public sealed class HexColorToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush FallbackBrush = CreateFrozen("#A1A1AA");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            var brush = TryCreateBrush(hex);
            if (brush is not null)
            {
                return brush;
            }
        }

        return FallbackBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static SolidColorBrush? TryCreateBrush(string hex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex)!;
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
        catch
        {
            return null;
        }
    }

    private static SolidColorBrush CreateFrozen(string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex)!;
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}