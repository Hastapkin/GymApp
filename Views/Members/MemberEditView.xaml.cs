using System.Windows;
using GymApp.ViewModels.Member;

namespace GymApp.Views.Members
{
    public partial class MemberEditView : Window
    {
        public MemberEditView(GymApp.Models.Member member)
        {
            InitializeComponent();
            var viewModel = new MemberEditViewModel(member);
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}