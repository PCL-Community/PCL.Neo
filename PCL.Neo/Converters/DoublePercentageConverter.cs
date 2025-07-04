using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PCL.Neo.Converters;

public class DoublePercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d && targetType.IsAssignableTo(typeof(string)))
            return $"{d * 100:0.##}%";
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
