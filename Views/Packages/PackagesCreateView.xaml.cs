using System.Windows;
using GymApp.ViewModels.Packages;

namespace GymApp.Views.Packages
{
    public partial class PackagesCreateView : Window
    {
        public PackagesCreateView()
        {
            InitializeComponent();
            var viewModel = new PackagesCreateViewModel();
            viewModel.RequestClose += (success) =>
            {
                DialogResult = success;
                Close();
            };
            DataContext = viewModel;
        }
    }
}