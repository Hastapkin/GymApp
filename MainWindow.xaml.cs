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
            else
            {
                // Nếu không có page nào đang hiển thị, chuyển về Members_Info
                MainFrame.Navigate(new Members_InfoListView());
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

    // ✅ KEYBOARD SHORTCUTS
    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        // Handle global keyboard shortcuts
        if (e.Key == System.Windows.Input.Key.F1) // Help
        {
            ShowHelpDialog();
            e.Handled = true;
        }
        else if (e.Key == System.Windows.Input.Key.F5) // Refresh
        {
            RefreshCurrentPage();
            e.Handled = true;
        }
        else if (e.Key == System.Windows.Input.Key.Escape) // Go to Dashboard
        {
            Dashboard_Click(null, null);
            e.Handled = true;
        }
        else if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.N: // Ctrl+N - New Member
                    MemberCreate_Click(null, null);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.M: // Ctrl+M - Members List
                    MemberList_Click(null, null);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.P: // Ctrl+P - Packages
                    PackageList_Click(null, null);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.T: // Ctrl+T - Membership Cards
                    MembershipList_Click(null, null);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.S: // Ctrl+S - Staff
                    StaffList_Click(null, null);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.Q: // Ctrl+Q - Quit
                    Exit_Click(null, null);
                    e.Handled = true;
                    break;
            }
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Show help dialog with keyboard shortcuts
    /// </summary>
    private void ShowHelpDialog()
    {
        var helpMessage = "🎯 PHÍM TẮT TRONG ỨNG DỤNG:\n\n" +
            "F1 - Hiển thị trợ giúp\n" +
            "F5 - Làm mới trang hiện tại\n" +
            "ESC - Về trang chủ\n\n" +
            "Ctrl + N - Thêm thành viên mới\n" +
            "Ctrl + M - Danh sách thành viên\n" +
            "Ctrl + P - Danh sách gói tập\n" +
            "Ctrl + T - Danh sách thẻ tập\n" +
            "Ctrl + S - Danh sách nhân viên\n" +
            "Ctrl + Q - Thoát ứng dụng\n\n" +
            "📱 LIÊN HỆ HỖ TRỢ:\n" +
            "Email: support@gymapp.com\n" +
            "Hotline: 1800-GYM-APP";

        MessageBox.Show(helpMessage, "Trợ giúp - Gym Management System",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Refresh current page content
    /// </summary>
    private void RefreshCurrentPage()
    {
        if (MainFrame.Content != null)
        {
            var currentPageType = MainFrame.Content.GetType();

            // Create new instance of current page
            if (currentPageType == typeof(DashboardView))
            {
                MainFrame.Navigate(new DashboardView());
                StatusText.Text = "Đã làm mới trang chủ";
            }
            else if (currentPageType == typeof(MemberListView))
            {
                MainFrame.Navigate(new MemberListView());
                StatusText.Text = "Đã làm mới danh sách thành viên";
            }
            else if (currentPageType == typeof(Members_InfoListView))
            {
                MainFrame.Navigate(new Members_InfoListView());
                StatusText.Text = "Đã làm mới thông tin thành viên";
            }
            else if (currentPageType == typeof(PackagesListView))
            {
                MainFrame.Navigate(new PackagesListView());
                StatusText.Text = "Đã làm mới danh sách gói tập";
            }
            else if (currentPageType == typeof(MembershipCardsListView))
            {
                MainFrame.Navigate(new MembershipCardsListView());
                StatusText.Text = "Đã làm mới danh sách thẻ tập";
            }
            else if (currentPageType == typeof(StaffListView))
            {
                MainFrame.Navigate(new StaffListView());
                StatusText.Text = "Đã làm mới danh sách nhân viên";
            }
        }
    }
}