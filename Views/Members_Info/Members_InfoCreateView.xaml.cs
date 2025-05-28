using System.Windows;
using System.Windows.Controls;
using GymApp.ViewModels.Members_Info;

namespace GymApp.Views.Members_Info
{
    /// <summary>
    /// Code-behind cho việc tạo member mới và thẻ tập đồng thời
    /// </summary>
    public partial class Members_InfoCreateView : Window
    {
        private Members_InfoCreateViewModel _viewModel;

        public Members_InfoCreateView()
        {
            InitializeComponent();

            // Khởi tạo ViewModel
            _viewModel = new Members_InfoCreateViewModel();

            // Xử lý sự kiện đóng cửa sổ
            _viewModel.RequestClose += OnRequestClose;

            // Gán DataContext
            DataContext = _viewModel;

            // Focus vào TextBox họ tên
            this.Loaded += OnWindowLoaded;
        }

        /// <summary>
        /// Xử lý khi cửa sổ được load
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Tìm TextBox họ tên và focus
            var fullNameTextBox = FindVisualChild<TextBox>(this, "FullNameTextBox");
            if (fullNameTextBox != null)
            {
                fullNameTextBox.Focus();
                fullNameTextBox.SelectAll();
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
        /// <param name="success">True nếu tạo thành công</param>
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
            // Ctrl+S để lưu
            else if (e.Key == System.Windows.Input.Key.S &&
                     (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
            {
                if (_viewModel?.SaveCommand?.CanExecute(null) == true)
                {
                    _viewModel.SaveCommand.Execute(null);
                }
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi gói tập - tự động tính ngày kết thúc
        /// </summary>
        private void PackageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedPackage != null)
            {
                // ViewModel sẽ tự động tính toán ngày kết thúc và giá
                System.Diagnostics.Debug.WriteLine($"Đã chọn gói: {_viewModel.SelectedPackage.PackageName}");
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi ngày bắt đầu
        /// </summary>
        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedPackage != null)
            {
                // ViewModel sẽ tự động tính lại ngày kết thúc
                System.Diagnostics.Debug.WriteLine($"Ngày bắt đầu: {_viewModel.MembershipStartDate:dd/MM/yyyy}");
            }
        }

        /// <summary>
        /// Validate dữ liệu trước khi lưu
        /// </summary>
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(_viewModel?.NewMember?.FullName))
            {
                MessageBox.Show("Vui lòng nhập họ tên thành viên!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_viewModel?.SelectedPackage == null)
            {
                MessageBox.Show("Vui lòng chọn gói tập!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_viewModel?.MembershipStartDate >= _viewModel?.MembershipEndDate)
            {
                MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}