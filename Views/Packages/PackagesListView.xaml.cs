using System.Windows.Controls;
using GymApp.ViewModels.Packages;
using GymApp.Views.Packages;

namespace GymApp.Views.Packages
{
    public partial class PackagesListView : Page
    {
        private PackagesListViewModel _viewModel;

        public PackagesListView()
        {
            InitializeComponent();
            _viewModel = new PackagesListViewModel();
            DataContext = _viewModel;
        }

        private void EditPackage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var package = button?.DataContext as GymApp.Models.Packages;

            if (package != null)
            {
                var editWindow = new PackagesEditView(package);
                if (editWindow.ShowDialog() == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
            }
        }

        private void ViewPackage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var package = button?.DataContext as GymApp.Models.Packages;

            if (package != null)
            {
                System.Windows.MessageBox.Show($"Thông tin chi tiết gói tập:\n\n" +
                    $"ID: {package.Id}\n" +
                    $"Tên gói: {package.PackageName}\n" +
                    $"Mô tả: {package.Description}\n" +
                    $"Thời hạn: {package.DurationDays} ngày\n" +
                    $"Giá: {package.Price:N0} VNĐ\n" +
                    $"Trạng thái: {(package.IsActive ? "Hoạt động" : "Không hoạt động")}\n" +
                    $"Ngày tạo: {package.CreatedDate:dd/MM/yyyy}",
                    "Chi tiết gói tập",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}