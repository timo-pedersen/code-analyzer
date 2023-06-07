using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public Visibility TrueValue { get; set; }
    public Visibility FalseValue { get; set; }

    public BoolToVisibilityConverter()
    {
        TrueValue = Visibility.Visible;
        FalseValue = Visibility.Collapsed;
    }

    public object Convert(object? value, Type targetType, 
        object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? TrueValue : FalseValue;

        return FalseValue;
    }

    public object ConvertBack(object? value, Type targetType, 
        object? parameter, CultureInfo culture)
    {
        if (Equals(value, TrueValue))
            return true;
        if (Equals(value, FalseValue))
            return false;

        return false;
    }
}