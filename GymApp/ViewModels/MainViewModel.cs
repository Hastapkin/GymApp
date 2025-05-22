using GymApp.Commands;
using GymApp.Views;
using System.Windows.Input;

namespace GymApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentPage;

        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public ICommand NavigateToMembersCommand { get; }
        public ICommand NavigateToMembershipCardsCommand { get; }
        public ICommand NavigateToStaffCommand { get; }

        public MainViewModel()
        {
            NavigateToMembersCommand = new RelayCommand(() =>
            {
                var page = new MembersPage();
                page.DataContext = new MembersViewModel();
                CurrentPage = page;
            });

            NavigateToMembershipCardsCommand = new RelayCommand(() =>
            {
                var page = new MembershipCardsPage();
                page.DataContext = new MembershipCardsViewModel();
                CurrentPage = page;
            });

            NavigateToStaffCommand = new RelayCommand(() =>
            {
                var page = new StaffPage();
                page.DataContext = new StaffViewModel();
                CurrentPage = page;
            });

            // Set default page
            var defaultPage = new MembersPage();
            defaultPage.DataContext = new MembersViewModel();
            CurrentPage = defaultPage;
        }
    }
}