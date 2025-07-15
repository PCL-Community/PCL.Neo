using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PCL.Neo.Utils;

public class BoolToOnlineStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isOnline)
        {
            return isOnline ? "正版账户" : "离线账户";
        }

        return "离线账户";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
