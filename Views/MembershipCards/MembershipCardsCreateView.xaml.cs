using System.Windows;
using GymApp.ViewModels.MembershipCards;

namespace GymApp.Views.MembershipCards
{
    public partial class MembershipCardsCreateView : Window
    {
        public MembershipCardsCreateView()
        {
            InitializeComponent();
            var viewModel = new MembershipCardsCreateViewModel();
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}