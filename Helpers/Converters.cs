using System.Globalization;
using System.Windows.Data;
using System.Collections;
using System.Linq;
using System.Windows;
using System;
using System.Windows.Media;

namespace GymApp.Helpers;

// ✅ CÁC CONVERTER CƠ BẢN
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

// ✅ CÁC CONVERTER SO SÁNH SỐ
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

// ✅ CÁC CONVERTER CHO COLLECTIONS
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

// ✅ CÁC CONVERTER CHO UI
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

// ✅ MEMBERSHIP HELPER METHODS (chuyển từ class riêng vào đây)
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

    /// <summary>
    /// Validate thông tin member trước khi tạo
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateMember(GymApp.Models.Member member)
    {
        if (member == null)
            return (false, "Thông tin thành viên không được để trống");

        if (string.IsNullOrWhiteSpace(member.FullName))
            return (false, "Vui lòng nhập họ tên");

        if (member.FullName.Length < 2)
            return (false, "Họ tên phải có ít nhất 2 ký tự");

        if (member.FullName.Length > 100)
            return (false, "Họ tên không được quá 100 ký tự");

        // Validate phone nếu có
        if (!string.IsNullOrWhiteSpace(member.Phone))
        {
            if (member.Phone.Length < 10 || member.Phone.Length > 11)
                return (false, "Số điện thoại phải có 10-11 chữ số");

            if (!System.Text.RegularExpressions.Regex.IsMatch(member.Phone, @"^[0-9]+$"))
                return (false, "Số điện thoại chỉ được chứa chữ số");
        }

        // Validate email nếu có
        if (!string.IsNullOrWhiteSpace(member.Email))
        {
            if (!IsValidEmail(member.Email))
                return (false, "Email không đúng định dạng");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Kiểm tra email hợp lệ
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Lấy icon emoji dựa trên trạng thái thẻ
    /// </summary>
    public static string GetStatusIcon(string membershipStatus, int daysRemaining)
    {
        if (membershipStatus == "Hết hạn" || daysRemaining < 0)
            return "🔴";

        if (daysRemaining <= 3)
            return "🔴";

        if (daysRemaining <= 7)
            return "🟡";

        if (daysRemaining <= 30)
            return "🟢";

        return "💚";
    }

    /// <summary>
    /// Tính toán ngày kết thúc dựa trên gói tập
    /// </summary>
    public static DateTime CalculateEndDate(DateTime startDate, int durationDays)
    {
        return startDate.AddDays(durationDays);
    }

    /// <summary>
    /// Tính giá gia hạn dựa trên số ngày
    /// </summary>
    public static decimal CalculateExtensionPrice(decimal originalPrice, int originalDays, int extensionDays)
    {
        if (originalDays <= 0) return 0;

        var dailyRate = originalPrice / originalDays;
        return Math.Round(dailyRate * extensionDays, 0);
    }

    /// <summary>
    /// Validate thông tin thẻ tập trước khi tạo
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateMembershipCard(GymApp.Models.MembershipCards membershipCard)
    {
        if (membershipCard == null)
            return (false, "Thông tin thẻ tập không được để trống");

        if (membershipCard.MemberId <= 0)
            return (false, "Vui lòng chọn thành viên");

        if (membershipCard.PackageId <= 0)
            return (false, "Vui lòng chọn gói tập");

        if (membershipCard.StartDate >= membershipCard.EndDate)
            return (false, "Ngày kết thúc phải sau ngày bắt đầu");

        if (membershipCard.Price <= 0)
            return (false, "Giá thẻ tập phải lớn hơn 0");

        if (membershipCard.StartDate.Date < DateTime.Now.Date.AddDays(-30))
            return (false, "Ngày bắt đầu không được quá xa trong quá khứ");

        if (membershipCard.EndDate.Date > DateTime.Now.Date.AddYears(2))
            return (false, "Ngày kết thúc không được quá 2 năm từ hiện tại");

        return (true, string.Empty);
    }

    /// <summary>
    /// Tạo ghi chú gia hạn
    /// </summary>
    public static string CreateExtensionNote(int extensionDays, decimal extensionPrice, string packageName = null)
    {
        var note = $"Gia hạn {extensionDays} ngày - {extensionPrice:N0} VNĐ ({DateTime.Now:dd/MM/yyyy})";

        if (!string.IsNullOrEmpty(packageName))
            note = $"{note} - {packageName}";

        return note;
    }

    /// <summary>
    /// Tính toán giảm giá cho gia hạn sớm
    /// </summary>
    public static decimal CalculateEarlyRenewalDiscount(int daysBeforeExpiry, decimal originalPrice)
    {
        // Giảm giá 5% nếu gia hạn trước 30 ngày
        if (daysBeforeExpiry >= 30)
            return originalPrice * 0.95m;

        // Giảm giá 3% nếu gia hạn trước 15 ngày
        if (daysBeforeExpiry >= 15)
            return originalPrice * 0.97m;

        return originalPrice;
    }

    /// <summary>
    /// Tạo mã thẻ tập tự động
    /// </summary>
    public static string GenerateMembershipCode(int memberId, int packageId)
    {
        var date = DateTime.Now.ToString("yyMMdd");
        return $"GYM{date}M{memberId:D4}P{packageId:D2}";
    }
}