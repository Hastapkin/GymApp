using System.Windows;
using GymApp.ViewModels.Members_Info;

namespace GymApp.Views.Members_Info
{
    public partial class Members_InfoCreateView : Window
    {
        public Members_InfoCreateView()
        {
            InitializeComponent();
            var viewModel = new Members_InfoCreateViewModel();
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}