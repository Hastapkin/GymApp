using System.Globalization;
using System.Windows.Data;
using System.Collections;
using System.Linq;
using System.Windows;
using System;

namespace GymApp.Helpers;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Hoạt động" : "Không hoạt động";
        }
        return "Không xác định";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == "Hoạt động";
    }
}

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy");
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (DateTime.TryParse(value?.ToString(), out DateTime result))
        {
            return result;
        }
        return DateTime.Now;
    }
}

public class NumberToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString("N0");
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (decimal.TryParse(value?.ToString(), out decimal result))
        {
            return result;
        }
        return 0m;
    }
}

// Converter cho so sánh số
public class LessThanConverter : IValueConverter
{
    public static readonly LessThanConverter Instance = new LessThanConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out int threshold))
        {
            return intValue < threshold && intValue >= 0;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LessThanEqualConverter : IValueConverter
{
    public static readonly LessThanEqualConverter Instance = new LessThanEqualConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out int threshold))
        {
            return intValue <= threshold;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class GreaterThanConverter : IValueConverter
{
    public static readonly GreaterThanConverter Instance = new GreaterThanConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue && parameter is string paramStr && int.TryParse(paramStr, out int threshold))
        {
            return intValue > threshold;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter cho collections
public class CountByStatusConverter : IValueConverter
{
    public static readonly CountByStatusConverter Instance = new CountByStatusConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable collection && parameter is string status)
        {
            return collection.Cast<GymApp.Models.Members_Info>()
                .Count(m => m.MembershipStatus == status);
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CountExpiringSoonConverter : IValueConverter
{
    public static readonly CountExpiringSoonConverter Instance = new CountExpiringSoonConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable collection)
        {
            return collection.Cast<GymApp.Models.Members_Info>()
                .Count(m => m.MembershipStatus == "Còn hạn" && m.DaysRemaining <= 7);
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter cho null checks
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter cho status colors
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status switch
            {
                "Hoạt động" => "#27AE60",
                "Hết hạn" => "#E74C3C",
                "Tạm ngưng" => "#F39C12",
                "Còn hạn" => "#27AE60",
                _ => "#95A5A6"
            };
        }
        return "#95A5A6";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Converter cho days remaining colors
public class DaysRemainingToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int days)
        {
            return days switch
            {
                <= 0 => "#E74C3C",      // Red - Hết hạn
                <= 7 => "#F39C12",      // Orange - Sắp hết hạn  
                <= 30 => "#F1C40F",     // Yellow - Cảnh báo
                _ => "#27AE60"          // Green - Còn nhiều
            };
        }
        return "#95A5A6";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Multi-value converter cho complex conditions
public class DaysRemainingMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is int days && values[1] is string status)
        {
            if (status == "Hết hạn") return "#E74C3C";
            if (days <= 0) return "#E74C3C";
            if (days <= 7) return "#F39C12";
            if (days <= 30) return "#F1C40F";
            return "#27AE60";
        }
        return "#95A5A6";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}