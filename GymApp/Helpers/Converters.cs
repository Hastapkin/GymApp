using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GymApp.Helpers
{
    // Name to Initials Converter
    public class NameToInitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name && !string.IsNullOrEmpty(name))
            {
                var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
                return parts.Length > 0 ? parts[0][0].ToString().ToUpper() : "?";
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Status to Color Converter
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "còn hạn" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                    "hoạt động" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                    "sắp hết hạn" => new SolidColorBrush(Color.FromRgb(251, 146, 60)),
                    "hết hạn" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                    _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
                };
            }
            return new SolidColorBrush(Color.FromRgb(107, 114, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Gender to Color Converter
    public class GenderToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string gender)
            {
                return gender.ToLower() switch
                {
                    "nam" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                    "nữ" => new SolidColorBrush(Color.FromRgb(236, 72, 153)),
                    _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
                };
            }
            return new SolidColorBrush(Color.FromRgb(107, 114, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Role to Color Converter
    public class RoleToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string role)
            {
                return role.ToLower() switch
                {
                    "quản lý" => new SolidColorBrush(Color.FromRgb(139, 92, 246)),
                    "thu ngân" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                    "huấn luyện viên" => new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                    "lao công" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                    _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
                };
            }
            return new SolidColorBrush(Color.FromRgb(107, 114, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}