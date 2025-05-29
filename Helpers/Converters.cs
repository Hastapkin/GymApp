using System.Globalization;
using System.Windows.Data;
using System.Collections;
using System.Linq;
using System.Windows;
using System;
using System.Windows.Media;

namespace GymApp.Helpers;

// CONVERTER
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
        if (value is int intValue)
        {
            return intValue.ToString("N0");
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (decimal.TryParse(value?.ToString()?.Replace(",", ""), out decimal result))
        {
            return result;
        }
        return 0m;
    }
}

// CONVERTER SO SÁNH SỐ
public class LessThanConverter : IValueConverter
{
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

// CONVERTER CHO COLLECTIONS
public class CountByStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable collection && parameter is string status)
        {
            try
            {
                return collection.Cast<GymApp.Models.Members_Info>()
                    .Count(m => m.MembershipStatus == status);
            }
            catch
            {
                return 0;
            }
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
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable collection)
        {
            try
            {
                return collection.Cast<GymApp.Models.Members_Info>()
                    .Count(m => m.MembershipStatus == "Còn hạn" && m.DaysRemaining <= 7);
            }
            catch
            {
                return 0;
            }
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// CONVERTER CHO UI
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
        {
            return Visibility.Collapsed;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            var color = status switch
            {
                "Hoạt động" => "#27AE60",
                "Hết hạn" => "#E74C3C", 
                "Tạm ngưng" => "#F39C12",
                "Còn hạn" => "#27AE60",
                _ => "#95A5A6"
            };

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DaysRemainingToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int days)
        {
            var color = days switch
            {
                <= 0 => "#E74C3C",      // Red - Hết hạn
                <= 7 => "#F39C12",      // Orange - Sắp hết hạn  
                <= 30 => "#F1C40F",     // Yellow - Cảnh báo
                _ => "#27AE60"          // Green - Còn nhiều
            };

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DaysRemainingMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is int days && values[1] is string status)
        {
            string color;
            if (status == "Hết hạn") color = "#E74C3C";
            else if (days <= 0) color = "#E74C3C";
            else if (days <= 7) color = "#F39C12";
            else if (days <= 30) color = "#F1C40F";
            else color = "#27AE60";

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// THÊM CÁC CONVERTER BỊ THIẾU
public class BoolToStringConverter2 : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Có" : "Không";
        }
        return "Không";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == "Có";
    }
}

// MEMBERSHIP HELPER METHODS (chuyển từ class riêng vào)
public static class MembershipHelper
{
    /// <summary>
    /// Tính số ngày còn lại của thẻ tập
    /// </summary>
    public static int CalculateDaysRemaining(DateTime endDate)
    {
        return (int)(endDate.Date - DateTime.Now.Date).TotalDays;
    }

    /// <summary>
    /// Xác định trạng thái thẻ tập
    /// </summary>
    public static string GetMembershipStatus(DateTime endDate)
    {
        var daysRemaining = CalculateDaysRemaining(endDate);
        return daysRemaining >= 0 ? "Còn hạn" : "Hết hạn";
    }

    /// <summary>
    /// Lấy màu sắc hiển thị dựa trên số ngày còn lại
    /// </summary>
    public static Brush GetStatusColor(int daysRemaining)
    {
        if (daysRemaining < 0) return new SolidColorBrush(Colors.Red);
        if (daysRemaining <= 3) return new SolidColorBrush(Colors.OrangeRed);
        if (daysRemaining <= 7) return new SolidColorBrush(Colors.Orange);
        if (daysRemaining <= 30) return new SolidColorBrush(Colors.Gold);
        return new SolidColorBrush(Colors.Green);
    }

    /// <summary>
    /// Format hiển thị thời gian còn lại
    /// </summary>
    public static string FormatTimeRemaining(int daysRemaining)
    {
        if (daysRemaining < 0) return "Đã hết hạn";
        if (daysRemaining == 0) return "Hết hạn hôm nay";
        if (daysRemaining == 1) return "Còn 1 ngày";
        return $"Còn {daysRemaining} ngày";
    }

    /// <summary>
    /// Kiểm tra thẻ tập có hợp lệ để check-in không
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateCheckIn(GymApp.Models.Members_Info memberInfo)
    {
        if (memberInfo == null)
            return (false, "Thông tin thành viên không hợp lệ");

        if (memberInfo.Status == "Tạm ngưng")
            return (false, "Thẻ tập đang bị tạm ngưng");

        if (memberInfo.MembershipStatus == "Hết hạn")
            return (false, $"Thẻ tập đã hết hạn từ ngày {memberInfo.EndDate:dd/MM/yyyy}");

        return (true, string.Empty);
    }

    /// <summary>
    /// Tạo thông báo check-in
    /// </summary>
    public static string CreateCheckInMessage(GymApp.Models.Members_Info memberInfo)
    {
        var timeRemaining = FormatTimeRemaining(memberInfo.DaysRemaining);

        return $"✅ CHECK-IN THÀNH CÔNG!\n\n" +
               $"👤 Thành viên: {memberInfo.FullName}\n" +
               $"📦 Gói tập: {memberInfo.PackageName}\n" +
               $"⏰ Thời gian: {DateTime.Now:HH:mm dd/MM/yyyy}\n" +
               $"📅 Thẻ tập: {timeRemaining}\n" +
               $"💳 Hết hạn: {memberInfo.EndDate:dd/MM/yyyy}";
    }

    /// <summary>
    /// Tạo thông báo cảnh báo thẻ sắp hết hạn
    /// </summary>
    public static string CreateExpirationWarning(GymApp.Models.Members_Info memberInfo)
    {
        if (memberInfo.DaysRemaining <= 0)
        {
            return $"⚠️ THẺ ĐÃ HẾT HẠN!\n\n" +
                   $"👤 Thành viên: {memberInfo.FullName}\n" +
                   $"📦 Gói tập: {memberInfo.PackageName}\n" +
                   $"📅 Hết hạn: {memberInfo.EndDate:dd/MM/yyyy}\n" +
                   $"⏰ Đã hết hạn {Math.Abs(memberInfo.DaysRemaining)} ngày\n\n" +
                   $"Vui lòng gia hạn thẻ để tiếp tục sử dụng dịch vụ!";
        }
        else if (memberInfo.DaysRemaining <= 7)
        {
            return $"⚠️ THẺ SẮP HẾT HẠN!\n\n" +
                   $"👤 Thành viên: {memberInfo.FullName}\n" +
                   $"📦 Gói tập: {memberInfo.PackageName}\n" +
                   $"📅 Hết hạn: {memberInfo.EndDate:dd/MM/yyyy}\n" +
                   $"⏰ Còn lại: {memberInfo.DaysRemaining} ngày\n\n" +
                   $"Bạn có muốn gia hạn thẻ ngay bây giờ không?";
        }

        return string.Empty;
    }
}