using System.Windows;
using GymApp.ViewModels.Member;

namespace GymApp.Views.Members
{
    public partial class MemberCreateView : Window
    {
        public MemberCreateView()
        {
            InitializeComponent();
            var viewModel = new MemberCreateViewModel();
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}