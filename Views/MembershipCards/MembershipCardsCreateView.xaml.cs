using System.Windows;
using GymApp.ViewModels.MembershipCards;

namespace GymApp.Views.MembershipCards
{
    /// <summary>
    /// Code-behind cho việc tạo thẻ tập mới cho member đã có sẵn
    /// </summary>
    public partial class MembershipCardsCreateView : Window
    {
        private MembershipCardsCreateViewModel _viewModel;

        public MembershipCardsCreateView()
        {
            InitializeComponent();

            // Khởi tạo ViewModel
            _viewModel = new MembershipCardsCreateViewModel();

            // Xử lý sự kiện đóng cửa sổ
            _viewModel.RequestClose += OnRequestClose;

            // Gán DataContext
            DataContext = _viewModel;

            // Focus vào ComboBox chọn member
            this.Loaded += (s, e) => {
                // Tìm ComboBox đầu tiên và focus
                var memberComboBox = this.FindName("MemberComboBox") as System.Windows.Controls.ComboBox;
                memberComboBox?.Focus();
            };
        }

        /// <summary>
        /// Xử lý sự kiện đóng cửa sổ từ ViewModel
        /// </summary>
        /// <param name="success">True nếu tạo thẻ thành công</param>
        private void OnRequestClose(bool success)
        {
            this.DialogResult = success;
            this.Close();
        }

        /// <summary>
        /// Xử lý khi đóng cửa sổ - cleanup resources
        /// </summary>
        /// <param name="e"></param>
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
        /// <param name="e"></param>
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
    }
}