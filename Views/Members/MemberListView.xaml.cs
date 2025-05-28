using System.Windows.Controls;
using GymApp.ViewModels.Member;
using GymApp.Views.Members;

namespace GymApp.Views.Members
{
    public partial class MemberListView : Page
    {
        private MemberListViewModel _viewModel;

        public MemberListView()
        {
            InitializeComponent();
            _viewModel = new MemberListViewModel();
            DataContext = _viewModel;
        }

        private void EditMember_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var member = button?.DataContext as GymApp.Models.Member;

            if (member != null)
            {
                var editWindow = new MemberEditView(member);
                if (editWindow.ShowDialog() == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
            }
        }

        private void ViewMember_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var member = button?.DataContext as GymApp.Models.Member;

            if (member != null)
            {
                System.Windows.MessageBox.Show($"Thông tin chi tiết thành viên:\n" +
                    $"ID: {member.Id}\n" +
                    $"Họ tên: {member.FullName}\n" +
                    $"Điện thoại: {member.Phone}\n" +
                    $"Email: {member.Email}\n" +
                    $"Địa chỉ: {member.Address}\n" +
                    $"Ghi chú: {member.Notes}",
                    "Chi tiết thành viên",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}