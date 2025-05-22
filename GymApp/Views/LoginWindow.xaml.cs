using System.Windows;
using GymApp.Services;
using GymApp.ViewModels;

namespace GymApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == "admin" && PasswordBox.Password == "admin")
            {
                try
                {
                    // Test database connection
                    var dbService = new DatabaseService();
                    dbService.InitializeDatabase();

                    var mainWindow = new MainWindow();
                    mainWindow.DataContext = new MainViewModel();
                    mainWindow.Show();
                    this.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"❌ Lỗi kết nối database:\n\n{ex.Message}", "Lỗi kết nối",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("❌ Sai tài khoản hoặc mật khẩu!\n\n💡 Thử: admin/admin", "Lỗi đăng nhập",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}