using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GymApp.ViewModels.Members_Info;
using GymApp.Views.Members_Info;
using GymApp.Helpers;

namespace GymApp.Views.Members_Info
{
    /// <summary>
    /// Code-behind cho danh sách thông tin thành viên và thẻ tập
    /// Bao gồm chức năng Check-in và quản lý thẻ tập
    /// </summary>
    public partial class Members_InfoListView : Page
    {
        private Members_InfoListViewModel _viewModel;

        public Members_InfoListView()
        {
            InitializeComponent();

            // Khởi tạo ViewModel
            _viewModel = new Members_InfoListViewModel();
            DataContext = _viewModel;

            // Setup UI khi load
            this.Loaded += OnPageLoaded;
        }

        /// <summary>
        /// Xử lý khi page được load
        /// </summary>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Focus vào search box
            SearchTextBox?.Focus();

            // Log thông tin
            System.Diagnostics.Debug.WriteLine("Members_InfoListView loaded successfully");
        }

        /// <summary>
        /// Xem thông tin chi tiết thành viên
        /// </summary>
        private void ViewMemberInfo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                ShowMemberDetailDialog(memberInfo);
            }
        }

        /// <summary>
        /// Hiển thị dialog thông tin chi tiết thành viên
        /// </summary>
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

        /// <summary>
        /// Gia hạn thẻ tập
        /// </summary>
        private void ExtendMembership_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                ShowExtendMembershipDialog(memberInfo);
            }
        }

        /// <summary>
        /// Hiển thị dialog gia hạn thẻ tập
        /// </summary>
        private void ShowExtendMembershipDialog(GymApp.Models.Members_Info memberInfo)
        {
            try
            {
                var extendWindow = new Members_InfoEditView(memberInfo);

                // Thiết lập owner để dialog hiển thị ở giữa
                extendWindow.Owner = Window.GetWindow(this);

                if (extendWindow.ShowDialog() == true)
                {
                    // Refresh danh sách sau khi gia hạn thành công
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

        /// <summary>
        /// Check-in thành viên
        /// </summary>
        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var memberInfo = button?.DataContext as GymApp.Models.Members_Info;

            if (memberInfo != null)
            {
                ProcessMemberCheckIn(memberInfo);
            }
        }

        /// <summary>
        /// Xử lý check-in thành viên
        /// </summary>
        private void ProcessMemberCheckIn(GymApp.Models.Members_Info memberInfo)
        {
            // Kiểm tra trạng thái thẻ tập bằng MembershipHelper
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

            // Cảnh báo nếu thẻ sắp hết hạn
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

            // Xác nhận check-in
            var confirmMessage = $"Xác nhận check-in cho:\n\n" +
                $"Thành viên: {memberInfo.FullName}\n" +
                $"Gói tập: {memberInfo.PackageName}\n" +
                $"Thẻ còn hiệu lực: {MembershipHelper.FormatTimeRemaining(memberInfo.DaysRemaining)}\n" +
                $"Thời gian: {DateTime.Now:HH:mm dd/MM/yyyy}";

            var confirmResult = MessageBox.Show(confirmMessage, "Xác nhận check-in",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.Yes)
            {
                // Gán selected member và thực hiện check-in
                _viewModel.SelectedMemberInfo = memberInfo;
                _viewModel.CheckInCommand.Execute(null);

                // Hiển thị thông báo thành công
                var successMessage = MembershipHelper.CreateCheckInMessage(memberInfo);
                MessageBox.Show(successMessage, "Check-in thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Xử lý search khi nhấn Enter
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _viewModel?.SearchCommand?.Execute(null);
            }
        }

        /// <summary>
        /// Xử lý double-click trên DataGrid row để xem chi tiết
        /// </summary>
        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewModel?.SelectedMemberInfo != null)
            {
                ShowMemberDetailDialog(_viewModel.SelectedMemberInfo);
            }
        }

        /// <summary>
        /// Xử lý right-click trên DataGrid để hiển thị context menu
        /// </summary>
        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem == null)
            {
                e.Handled = true;
                return;
            }

            // Tạo context menu động
            CreateContextMenu(dataGrid);
        }

        /// <summary>
        /// Tạo context menu cho DataGrid
        /// </summary>
        private void CreateContextMenu(DataGrid dataGrid)
        {
            var contextMenu = new ContextMenu();

            // Menu item: Xem chi tiết
            var viewMenuItem = new MenuItem
            {
                Header = "Xem chi tiết",
                Icon = new TextBlock { Text = "👁", FontSize = 12 }
            };
            viewMenuItem.Click += (s, e) => {
                if (_viewModel?.SelectedMemberInfo != null)
                    ShowMemberDetailDialog(_viewModel.SelectedMemberInfo);
            };
            contextMenu.Items.Add(viewMenuItem);

            // Separator
            contextMenu.Items.Add(new Separator());

            // Menu item: Check-in
            var checkinMenuItem = new MenuItem
            {
                Header = "Check-in",
                Icon = new TextBlock { Text = "✅", FontSize = 12 }
            };
            checkinMenuItem.Click += (s, e) => {
                if (_viewModel?.SelectedMemberInfo != null)
                    ProcessMemberCheckIn(_viewModel.SelectedMemberInfo);
            };
            contextMenu.Items.Add(checkinMenuItem);

            // Menu item: Gia hạn
            var extendMenuItem = new MenuItem
            {
                Header = "Gia hạn thẻ tập",
                Icon = new TextBlock { Text = "⏰", FontSize = 12 }
            };
            extendMenuItem.Click += (s, e) => {
                if (_viewModel?.SelectedMemberInfo != null)
                    ShowExtendMembershipDialog(_viewModel.SelectedMemberInfo);
            };
            contextMenu.Items.Add(extendMenuItem);

            // Gán context menu cho DataGrid
            dataGrid.ContextMenu = contextMenu;
        }

        /// <summary>
        /// Highlight các thẻ tập sắp hết hạn
        /// </summary>
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var memberInfo = e.Row.DataContext as GymApp.Models.Members_Info;
            if (memberInfo != null)
            {
                // Đổi màu background dựa trên trạng thái thẻ tập
                if (memberInfo.MembershipStatus == "Hết hạn")
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightCoral);
                }
                else if (memberInfo.DaysRemaining <= 7 && memberInfo.DaysRemaining > 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightYellow);
                }
                else if (memberInfo.MembershipStatus == "Còn hạn")
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightGreen);
                }
            }
        }

        /// <summary>
        /// Refresh danh sách với phím F5
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                _viewModel?.RefreshCommand?.Execute(null);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Xử lý khi rời khỏi page - cleanup
        /// </summary>
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Cleanup nếu cần thiết
            System.Diagnostics.Debug.WriteLine("Navigated away from Members_InfoListView");
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Xử lý khi navigate đến page này
        /// </summary>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Refresh data khi navigate đến page
            _viewModel?.RefreshCommand?.Execute(null);
            base.OnNavigatedTo(e);
        }
    }
}