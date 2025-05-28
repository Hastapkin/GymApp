using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymApp.ViewModels.Members_Info;
using GymApp.Views.Members_Info;
using GymApp.Helpers;

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
            this.Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox?.Focus();
            System.Diagnostics.Debug.WriteLine("Members_InfoListView loaded successfully");
        }

        // ✅ Event handlers được khai báo trong XAML
        private void ViewMemberInfo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                ShowMemberDetailDialog(memberInfo);
            }
        }

        private void ExtendMembership_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                ShowExtendMembershipDialog(memberInfo);
            }
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                ProcessMemberCheckIn(memberInfo);
            }
        }

        private void ShowMemberDetailDialog(GymApp.Models.Members_Info memberInfo)
        {
            var statusMessage = memberInfo.MembershipStatus == "Còn hạn"
                ? $"Còn {memberInfo.DaysRemaining} ngày"
                : "Đã hết hạn";

            var detailMessage = $"Thông tin chi tiết thành viên:\n\n" +
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
                $"Tình trạng: {statusMessage}";

            MessageBox.Show(detailMessage, "Chi tiết thành viên",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowExtendMembershipDialog(GymApp.Models.Members_Info memberInfo)
        {
            try
            {
                var extendWindow = new Members_InfoEditView(memberInfo);
                extendWindow.Owner = Window.GetWindow(this);

                if (extendWindow.ShowDialog() == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                    MessageBox.Show($"Gia hạn thẻ tập thành công cho {memberInfo.FullName}!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở cửa sổ gia hạn: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessMemberCheckIn(GymApp.Models.Members_Info memberInfo)
        {
            var validationResult = MembershipHelper.ValidateCheckIn(memberInfo);
            if (!validationResult.IsValid)
            {
                var warningMessage = MembershipHelper.CreateExpirationWarning(memberInfo);
                if (!string.IsNullOrEmpty(warningMessage))
                {
                    var result = MessageBox.Show(warningMessage, "Cảnh báo thẻ tập",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes && memberInfo.MembershipStatus == "Hết hạn")
                    {
                        ShowExtendMembershipDialog(memberInfo);
                    }
                }
                else
                {
                    MessageBox.Show(validationResult.ErrorMessage, "Không thể check-in",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            if (memberInfo.DaysRemaining <= 7 && memberInfo.DaysRemaining > 0)
            {
                var warningMessage = MembershipHelper.CreateExpirationWarning(memberInfo);
                if (!string.IsNullOrEmpty(warningMessage))
                {
                    var warningResult = MessageBox.Show(warningMessage, "Thẻ sắp hết hạn",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (warningResult != MessageBoxResult.Yes)
                        return;
                }
            }

            var confirmMessage = $"Xác nhận check-in cho:\n\n" +
                $"Thành viên: {memberInfo.FullName}\n" +
                $"Gói tập: {memberInfo.PackageName}\n" +
                $"Thẻ còn hiệu lực: {MembershipHelper.FormatTimeRemaining(memberInfo.DaysRemaining)}\n" +
                $"Thời gian: {DateTime.Now:HH:mm dd/MM/yyyy}";

            var confirmResult = MessageBox.Show(confirmMessage, "Xác nhận check-in",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.Yes)
            {
                _viewModel.SelectedMemberInfo = memberInfo;
                _viewModel.CheckInCommand.Execute(null);

                var successMessage = MembershipHelper.CreateCheckInMessage(memberInfo);
                MessageBox.Show(successMessage, "Check-in thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                _viewModel?.RefreshCommand?.Execute(null);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.RefreshCommand?.Execute(null);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Members_InfoListView unloaded");
        }
    }
}