using System.Windows;
using GymApp.ViewModels.Packages;

namespace GymApp.Views.Packages
{
    public partial class PackagesEditView : Window
    {
        public PackagesEditView(GymApp.Models.Packages package)
        {
            InitializeComponent();
            var viewModel = new PackagesEditViewModel(package);
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}