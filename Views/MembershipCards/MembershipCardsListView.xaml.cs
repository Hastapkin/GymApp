using System;
using System.Windows.Controls;
using GymApp.ViewModels.MembershipCards;
using GymApp.Views.MembershipCards;

namespace GymApp.Views.MembershipCards
{
    public partial class MembershipCardsListView : Page
    {
        private MembershipCardsListViewModel _viewModel;

        public MembershipCardsListView()
        {
            InitializeComponent();
            _viewModel = new MembershipCardsListViewModel();
            DataContext = _viewModel;
        }

        private void EditMembershipCard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var membershipCard = button?.DataContext as GymApp.Models.MembershipCards;

            if (membershipCard != null)
            {
                var editWindow = new MembershipCardsEditView(membershipCard);
                if (editWindow.ShowDialog() == true)
                {
                    _viewModel.RefreshCommand.Execute(null);
                }
            }
        }

        private void ViewMembershipCard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var membershipCard = button?.DataContext as GymApp.Models.MembershipCards;

            if (membershipCard != null)
            {
                var daysRemaining = (membershipCard.EndDate - System.DateTime.Now).Days;
                var statusColor = daysRemaining > 0 ? "Còn hiệu lực" : "Đã hết hạn";

                System.Windows.MessageBox.Show($"Thông tin chi tiết thẻ tập:\n\n" +
                    $"ID: {membershipCard.Id}\n" +
                    $"Thành viên: {membershipCard.MemberName}\n" +
                    $"Gói tập: {membershipCard.PackageName}\n" +
                    $"Thời gian: {membershipCard.StartDate:dd/MM/yyyy} - {membershipCard.EndDate:dd/MM/yyyy}\n" +
                    $"Giá: {membershipCard.Price:N0} VNĐ\n" +
                    $"Phương thức thanh toán: {membershipCard.PaymentMethod}\n" +
                    $"Trạng thái: {membershipCard.Status}\n" +
                    $"Tình trạng: {statusColor}\n" +
                    $"Số ngày còn lại: {Math.Max(0, daysRemaining)} ngày\n" +
                    $"Người tạo: {membershipCard.CreatedBy}\n" +
                    $"Ngày tạo: {membershipCard.CreatedDate:dd/MM/yyyy HH:mm}\n" +
                    $"Ghi chú: {membershipCard.Notes}",
                    "Chi tiết thẻ tập",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
    }
}