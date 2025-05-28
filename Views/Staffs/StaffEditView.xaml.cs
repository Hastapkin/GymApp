using System.Windows;
using GymApp.ViewModels.Staff;

namespace GymApp.Views.Staffs
{
    public partial class StaffEditView : Window
    {
        public StaffEditView(GymApp.Models.Staff staff)
        {
            InitializeComponent();
            var viewModel = new StaffEditViewModel(staff);
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}