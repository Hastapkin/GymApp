using GymApp.Views;
using GymApp.Views.Members;
using GymApp.Views.Packages;
using GymApp.Views.MembershipCards;
using GymApp.Views.Staffs;
using GymApp.Views.Members_Info;
using System.Windows;

namespace GymApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Load dashboard by default
        MainFrame.Navigate(new DashboardView());
        StatusText.Text = "Đã tải trang chủ";
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

    private void PackageList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new PackagesListView());
        StatusText.Text = "Đã tải danh sách gói tập";
    }

    private void PackageCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new PackagesCreateView();
        createWindow.ShowDialog();
    }

    private void MembershipList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new MembershipCardsListView());
        StatusText.Text = "Đã tải danh sách thẻ tập";
    }

    private void MembershipCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new MembershipCardsCreateView();
        createWindow.ShowDialog();
    }

    private void StaffList_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new StaffListView());
        StatusText.Text = "Đã tải danh sách nhân viên";
    }

    private void StaffCreate_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new StaffCreateView();
        createWindow.ShowDialog();
    }
}