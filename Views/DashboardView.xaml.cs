using System.Windows.Controls;
using GymApp.ViewModels;

namespace GymApp.Views
{
    public partial class DashboardView : Page
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}