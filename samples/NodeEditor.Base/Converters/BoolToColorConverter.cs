using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace NodeEditorDemo.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Brushes.Green : Brushes.Red;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color == Colors.Green;
        }

        return false;
    }
}
