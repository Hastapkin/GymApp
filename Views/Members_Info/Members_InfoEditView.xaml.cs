using System.Windows;
using System.Windows.Controls;
using GymApp.ViewModels.Members_Info;

namespace GymApp.Views.Members_Info
{
    /// <summary>
    /// Code-behind cho việc gia hạn thẻ tập
    /// </summary>
    public partial class Members_InfoEditView : Window
    {
        private Members_InfoEditViewModel _viewModel;

        public Members_InfoEditView(GymApp.Models.Members_Info memberInfo)
        {
            InitializeComponent();

            // Khởi tạo ViewModel với thông tin member
            _viewModel = new Members_InfoEditViewModel(memberInfo);

            // Xử lý sự kiện đóng cửa sổ
            _viewModel.RequestClose += OnRequestClose;

            // Gán DataContext
            DataContext = _viewModel;

            // Setup UI khi load
            this.Loaded += OnWindowLoaded;
        }

        /// <summary>
        /// Xử lý khi cửa sổ được load
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Focus vào ComboBox chọn gói gia hạn
            var packageComboBox = FindVisualChild<ComboBox>(this, "PackageComboBox");
            packageComboBox?.Focus();

            // Hiển thị thông tin hiện tại
            DisplayCurrentMembershipInfo();
        }

        /// <summary>
        /// Hiển thị thông tin thẻ tập hiện tại
        /// </summary>
        private void DisplayCurrentMembershipInfo()
        {
            if (_viewModel?.MemberInfo != null)
            {
                var memberInfo = _viewModel.MemberInfo;
                var statusMessage = memberInfo.MembershipStatus == "Còn hạn"
                    ? $"Còn {memberInfo.DaysRemaining} ngày"
                    : "Đã hết hạn";

                System.Diagnostics.Debug.WriteLine($"Thông tin thẻ tập hiện tại:");
                System.Diagnostics.Debug.WriteLine($"- Thành viên: {memberInfo.FullName}");
                System.Diagnostics.Debug.WriteLine($"- Gói hiện tại: {memberInfo.PackageName}");
                System.Diagnostics.Debug.WriteLine($"- Hết hạn: {memberInfo.EndDate:dd/MM/yyyy}");
                System.Diagnostics.Debug.WriteLine($"- Trạng thái: {statusMessage}");
            }
        }

        /// <summary>
        /// Tìm child control theo tên
        /// </summary>
        private T FindVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T && ((FrameworkElement)child).Name == name)
                {
                    return (T)child;
                }

                T childOfChild = FindVisualChild<T>(child, name);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        /// <summary>
        /// Xử lý sự kiện đóng cửa sổ từ ViewModel
        /// </summary>
        /// <param name="success">True nếu gia hạn thành công</param>
        private void OnRequestClose(bool success)
        {
            this.DialogResult = success;
            this.Close();
        }

        /// <summary>
        /// Xử lý khi đóng cửa sổ - cleanup resources
        /// </summary>
        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.RequestClose -= OnRequestClose;
            }
            base.OnClosed(e);
        }

        /// <summary>
        /// Xử lý phím tắt
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // ESC để hủy
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (_viewModel?.CancelCommand?.CanExecute(null) == true)
                {
                    _viewModel.CancelCommand.Execute(null);
                }
            }
            // Ctrl+E để gia hạn
            else if (e.Key == System.Windows.Input.Key.E &&
                     (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
            {
                if (_viewModel?.ExtendCommand?.CanExecute(null) == true)
                {
                    _viewModel.ExtendCommand.Execute(null);
                }
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi gói gia hạn
        /// </summary>
        private void PackageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedPackage != null)
            {
                var package = _viewModel.SelectedPackage;
                System.Diagnostics.Debug.WriteLine($"Chọn gói gia hạn: {package.PackageName} - {package.DurationDays} ngày - {package.Price:N0} VNĐ");

                // Hiển thị thông tin gia hạn
                UpdateExtensionSummary();
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi ngày kết thúc mới
        /// </summary>
        private void NewEndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateExtensionSummary();
        }

        /// <summary>
        /// Cập nhật thông tin tóm tắt gia hạn
        /// </summary>
        private void UpdateExtensionSummary()
        {
            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"Tóm tắt gia hạn:");
                System.Diagnostics.Debug.WriteLine($"- Từ: {_viewModel.MemberInfo.EndDate:dd/MM/yyyy}");
                System.Diagnostics.Debug.WriteLine($"- Đến: {_viewModel.NewEndDate:dd/MM/yyyy}");
                System.Diagnostics.Debug.WriteLine($"- Thêm: {_viewModel.ExtensionDays} ngày");
                System.Diagnostics.Debug.WriteLine($"- Giá: {_viewModel.ExtensionPrice:N0} VNĐ");
            }
        }

        /// <summary>
        /// Validate dữ liệu trước khi gia hạn
        /// </summary>
        private bool ValidateExtension()
        {
            if (_viewModel?.NewEndDate <= _viewModel?.MemberInfo?.EndDate)
            {
                MessageBox.Show("Ngày gia hạn phải sau ngày kết thúc hiện tại!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_viewModel?.ExtensionPrice <= 0)
            {
                MessageBox.Show("Giá gia hạn phải lớn hơn 0!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Hiển thị xác nhận gia hạn
        /// </summary>
        private void ShowExtensionConfirmation()
        {
            if (_viewModel?.MemberInfo != null)
            {
                var confirmMessage = $"Xác nhận gia hạn thẻ tập?\n\n" +
                    $"Thành viên: {_viewModel.MemberInfo.FullName}\n" +
                    $"Từ: {_viewModel.MemberInfo.EndDate:dd/MM/yyyy}\n" +
                    $"Đến: {_viewModel.NewEndDate:dd/MM/yyyy}\n" +
                    $"Thêm: {_viewModel.ExtensionDays} ngày\n" +
                    $"Giá: {_viewModel.ExtensionPrice:N0} VNĐ";

                var result = MessageBox.Show(confirmMessage, "Xác nhận gia hạn",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes && ValidateExtension())
                {
                    _viewModel?.ExtendCommand?.Execute(null);
                }
            }
        }
    }
}