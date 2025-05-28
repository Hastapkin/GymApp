using System.Windows;
using GymApp.ViewModels.Staff;

namespace GymApp.Views.Staffs
{
    public partial class StaffCreateView : Window
    {
        public StaffCreateView()
        {
            InitializeComponent();
            var viewModel = new StaffCreateViewModel();
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}