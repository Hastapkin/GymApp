using GymApp.Views;
using GymApp.Views.Members;
using GymApp.Views.Packages;
using GymApp.Views.MembershipCards;
using GymApp.Views.Staffs;
using GymApp.Views.Members_Info;
using System.Windows;
using System.Windows.Threading;
using System;

namespace GymApp;

public partial class MainWindow : Window
{
    private DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        // Load dashboard by default
        MainFrame.Navigate(new DashboardView());
        StatusText.Text = "Đã tải trang chủ";

        // Setup timer for current time display
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        CurrentTimeText.Text = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new DashboardView());
        StatusText.Text = "Đã tải trang chủ";
    }

    private void MemberList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new MemberListView());
        StatusText.Text = "Đã tải danh sách thành viên";
    }

    private void MemberCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new MemberCreateView();
        if (createWindow.ShowDialog() == true)
        {
            // Refresh member list if currently displayed
            if (MainFrame.Content is MemberListView)
            {
                MainFrame.Navigate(new MemberListView());
                StatusText.Text = "Đã thêm thành viên mới và làm mới danh sách";
            }
        }
    }

    private void MemberInfo_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Members_InfoListView());
        StatusText.Text = "Đã tải thông tin thành viên";
    }

    // ✅ NEW: Tạo thành viên và thẻ tập cùng lúc
    private void MemberInfoCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new Members_InfoCreateView();
        if (createWindow.ShowDialog() == true)
        {
            // Refresh both member info and member list if displayed
            if (MainFrame.Content is Members_InfoListView)
            {
                MainFrame.Navigate(new Members_InfoListView());
                StatusText.Text = "Đã tạo thành viên và thẻ tập mới";
            }
            else if (MainFrame.Content is MemberListView)
            {
                MainFrame.Navigate(new MemberListView());
                StatusText.Text = "Đã tạo thành viên và thẻ tập mới";
            }
        }
    }

    private void PackageList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PackagesListView());
        StatusText.Text = "Đã tải danh sách gói tập";
    }

    private void PackageCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new PackagesCreateView();
        if (createWindow.ShowDialog() == true)
        {
            // Refresh package list if currently displayed
            if (MainFrame.Content is PackagesListView)
            {
                MainFrame.Navigate(new PackagesListView());
                StatusText.Text = "Đã thêm gói tập mới và làm mới danh sách";
            }
        }
    }

    private void MembershipList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new MembershipCardsListView());
        StatusText.Text = "Đã tải danh sách thẻ tập";
    }

    private void MembershipCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new MembershipCardsCreateView();
        if (createWindow.ShowDialog() == true)
        {
            // Refresh membership list if currently displayed
            if (MainFrame.Content is MembershipCardsListView)
            {
                MainFrame.Navigate(new MembershipCardsListView());
                StatusText.Text = "Đã tạo thẻ tập mới và làm mới danh sách";
            }
        }
    }

    private void StaffList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new StaffListView());
        StatusText.Text = "Đã tải danh sách nhân viên";
    }

    private void StaffCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new StaffCreateView();
        if (createWindow.ShowDialog() == true)
        {
            // Refresh staff list if currently displayed
            if (MainFrame.Content is StaffListView)
            {
                MainFrame.Navigate(new StaffListView());
                StatusText.Text = "Đã thêm nhân viên mới và làm mới danh sách";
            }
        }
    }

    // ✅ NEW: Logout functionality
    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?",
                                   "Xác nhận đăng xuất",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _timer?.Stop();

            // Show login window
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close current window
            this.Close();
        }
    }

    // ✅ NEW: Exit functionality
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Bạn có chắc chắn muốn thoát ứng dụng?",
                                   "Xác nhận thoát",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _timer?.Stop();
            Application.Current.Shutdown();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer?.Stop();
        base.OnClosed(e);
    }
}