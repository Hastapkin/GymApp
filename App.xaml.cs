using System.Windows;
using GymApp.Data;
using GymApp.Views;

namespace GymApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Test database connection on startup
        var dbContext = new DbContext();
        if (!dbContext.TestConnection())
        {
            MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu!\n" +
                          "Vui lòng kiểm tra:\n" +
                          "- Oracle Database đã được khởi động\n" +
                          "- Connection string trong appsettings.json\n" +
                          "- User C##GymApp đã được tạo với mật khẩu a123",
                          "Lỗi kết nối database",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);

            // Still allow app to start for development
        }

        // IMPROVED: Start with Login Window instead of MainWindow
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        // FIX: Set proper shutdown mode
        ShutdownMode = ShutdownMode.OnLastWindowClose;
    }
}