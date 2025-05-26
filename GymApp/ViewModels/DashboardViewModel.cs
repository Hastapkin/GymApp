using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Helpers;
using GymApp.ViewModels.Member;
using GymApp.ViewModels.Membership;
using GymApp.ViewModels.Staff;

namespace GymApp.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private object? _currentView;
        private string _currentUser = string.Empty;

        public DashboardViewModel()
        {
            NavigateToMembersCommand = new RelayCommand(NavigateToMembers);
            NavigateToMembershipsCommand = new RelayCommand(NavigateToMemberships);
            NavigateToStaffCommand = new RelayCommand(NavigateToStaff);
            LogoutCommand = new RelayCommand(Logout);

            // Initialize with Members view by default
            CurrentView = new MemberListViewModel();
        }

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public string CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        public ICommand NavigateToMembersCommand { get; }
        public ICommand NavigateToMembershipsCommand { get; }
        public ICommand NavigateToStaffCommand { get; }
        public ICommand LogoutCommand { get; }

        public event Action? LogoutRequested;

        private void NavigateToMembers(object? parameter)
        {
            CurrentView = new MemberListViewModel();
        }

        private void NavigateToMemberships(object? parameter)
        {
            CurrentView = new MembershipListViewModel();
        }

        private void NavigateToStaff(object? parameter)
        {
            CurrentView = new StaffListViewModel();
        }

        private void Logout(object? parameter)
        {
            LogoutRequested?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}