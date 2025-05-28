using System.Windows.Controls;
using GymApp.ViewModels.Staff;
using GymApp.Views.Staffs;

namespace GymApp.Views.Staffs
{
    public partial class StaffListView : Page
    {
        private StaffListViewModel _viewModel;

        public StaffListView()
        {
            InitializeComponent();
            _viewModel = new StaffListViewModel();
            DataContext = _viewModel;
        }

        private void EditStaff_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var staff = button?.DataContext as GymApp.Models.Staff;

            if (staff != null)
            {
                var editWindow = new StaffEditView(staff);
                if (editWindow.ShowDialog() == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
            }
        }

        private void ViewStaff_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var staff = button?.DataContext as GymApp.Models.Staff;

            if (staff != null)
            {
                var workingDays = (System.DateTime.Now - staff.StartDate).Days;
                var workingYears = workingDays / 365.0;

                System.Windows.MessageBox.Show($"Thông tin chi tiết nhân viên:\n\n" +
                    $"ID: {staff.Id}\n" +
                    $"Họ tên: {staff.FullName}\n" +
                    $"Điện thoại: {staff.Phone}\n" +
                    $"Email: {staff.Email}\n" +
                    $"Chức vụ: {staff.Role}\n" +
                    $"Ngày bắt đầu: {staff.StartDate:dd/MM/yyyy}\n" +
                    $"Thời gian làm việc: {workingDays} ngày ({workingYears:F1} năm)\n" +
                    $"Lương: {staff.Salary:N0} VNĐ\n" +
                    $"Địa chỉ: {staff.Address}\n" +
                    $"Trạng thái: {(staff.IsActive ? "Đang làm việc" : "Đã nghỉ việc")}\n" +
                    $"Ngày tạo hồ sơ: {staff.CreatedDate:dd/MM/yyyy HH:mm}\n" +
                    $"Ghi chú: {staff.Notes}",
                    "Chi tiết nhân viên",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}