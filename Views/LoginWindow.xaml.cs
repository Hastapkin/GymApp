using System.Windows;
using System.Windows.Controls;
using GymApp.ViewModels;

namespace GymApp.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();

            // FIX: Subscribe to LoginSuccessful event properly
            _viewModel.LoginSuccessful += OnLoginSuccessful;

            DataContext = _viewModel;

            // FIX: Set default credentials for convenience
            _viewModel.Username = "admin";
            _viewModel.Password = "admin";
            PasswordBox.Password = "admin";

            // Focus on username textbox
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                // FIX: Update viewmodel password when user types
                _viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void OnLoginSuccessful(string username)
        {
            try
            {
                // Open main window
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Close login window
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi mở cửa sổ chính: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // FIX: Properly dispose of event subscription
        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LoginSuccessful -= OnLoginSuccessful;
            }
            base.OnClosed(e);
        }
    }
}