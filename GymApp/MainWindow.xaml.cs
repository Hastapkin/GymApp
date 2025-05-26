using System.Windows;
using GymApp.ViewModels;
using GymApp.Views;

namespace GymApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowLoginView();
        }

        private void ShowLoginView()
        {
            var loginViewModel = new LoginViewModel();
            loginViewModel.LoginSuccessful += OnLoginSuccessful;

            var loginView = new LoginView
            {
                DataContext = loginViewModel
            };

            MainContentControl.Content = loginView;
        }

        private void OnLoginSuccessful(string username)
        {
            var dashboardViewModel = new DashboardViewModel
            {
                CurrentUser = username
            };
            dashboardViewModel.LogoutRequested += OnLogoutRequested;

            var dashboardView = new DashboardView
            {
                DataContext = dashboardViewModel
            };

            MainContentControl.Content = dashboardView;
        }

        private void OnLogoutRequested()
        {
            ShowLoginView();
        }
    }
}