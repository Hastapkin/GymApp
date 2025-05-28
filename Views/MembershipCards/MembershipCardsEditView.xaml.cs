using System.Windows;
using GymApp.ViewModels.MembershipCards;

namespace GymApp.Views.MembershipCards
{
    public partial class MembershipCardsEditView : Window
    {
        public MembershipCardsEditView(GymApp.Models.MembershipCards membershipCard)
        {
            InitializeComponent();
            var viewModel = new MembershipCardsEditViewModel(membershipCard);
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}