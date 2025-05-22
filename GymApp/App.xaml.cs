using System.Windows;

namespace GymApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set global exception handling
            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Đã xảy ra lỗi không mong muốn: {args.Exception.Message}",
                    "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}