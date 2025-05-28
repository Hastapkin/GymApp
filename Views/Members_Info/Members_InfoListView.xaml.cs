using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GymApp.ViewModels.Members_Info;
using GymApp.Views.Members_Info;

namespace GymApp.Views.Members_Info
{
    public partial class Members_InfoListView : Page
    {
        private Members_InfoListViewModel _viewModel;

        public Members_InfoListView()
        {
            InitializeComponent();
            _viewModel = new Members_InfoListViewModel();
            DataContext = _viewModel;
        }

        private void ViewMemberInfo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                var statusMessage = memberInfo.MembershipStatus == "Còn hạn"
                    ? $"Còn {memberInfo.DaysRemaining} ngày"
                    : "Đã hết hạn";

                MessageBox.Show($"Thông tin chi tiết thành viên:\n\n" +
                    $"ID: {memberInfo.Id}\n" +
                    $"Họ tên: {memberInfo.FullName}\n" +
                    $"Điện thoại: {memberInfo.Phone}\n" +
                    $"Email: {memberInfo.Email}\n" +
                    $"Giới tính: {memberInfo.Gender}\n" +
                    $"Ngày tham gia: {memberInfo.JoinDate:dd/MM/yyyy}\n\n" +
                    $"THÔNG TIN THẺ TẬP:\n" +
                    $"Gói tập: {memberInfo.PackageName}\n" +
                    $"Thời gian: {memberInfo.StartDate:dd/MM/yyyy} - {memberInfo.EndDate:dd/MM/yyyy}\n" +
                    $"Giá: {memberInfo.Price:N0} VNĐ\n" +
                    $"Trạng thái: {memberInfo.Status}\n" +
                    $"Tình trạng: {statusMessage}",
                    "Chi tiết thành viên",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ExtendMembership_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                var extendWindow = new Members_InfoEditView(memberInfo);
                if (extendWindow.ShowDialog() == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
            }
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                _viewModel.SelectedMemberInfo = memberInfo;
                _viewModel.CheckInCommand.Execute(null);
            }
        }
    }

    // Converter classes for the XAML bindings
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
}