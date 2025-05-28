using System.Windows;
using GymApp.ViewModels.Members_Info;

namespace GymApp.Views.Members_Info
{
    public partial class Members_InfoEditView : Window
    {
        public Members_InfoEditView(GymApp.Models.Members_Info memberInfo)
        {
            InitializeComponent();
            var viewModel = new Members_InfoEditViewModel(memberInfo);
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}