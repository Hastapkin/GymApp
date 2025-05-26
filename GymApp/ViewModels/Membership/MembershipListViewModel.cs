using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;

namespace GymApp.ViewModels.Membership
{
    public class MembershipListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Membership> _memberships;
        private Models.Membership? _selectedMembership;
        private object? _currentView;

        public MembershipListViewModel()
        {
            _dbContext = new DbContext();
            _memberships = new ObservableCollection<Models.Membership>();

            CreateMembershipCommand = new RelayCommand(CreateMembership);
            EditMembershipCommand = new RelayCommand(EditMembership, CanEditMembership);
            DeleteMembershipCommand = new RelayCommand(DeleteMembership, CanDeleteMembership);
            BackCommand = new RelayCommand(Back);

            LoadMemberships();
        }

        public ObservableCollection<Models.Membership> Memberships
        {
            get => _memberships;
            set
            {
                _memberships = value;
                OnPropertyChanged(nameof(Memberships));
            }
        }

        public Models.Membership? SelectedMembership
        {
            get => _selectedMembership;
            set
            {
                _selectedMembership = value;
                OnPropertyChanged(nameof(SelectedMembership));
            }
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

        public ICommand CreateMembershipCommand { get; }
        public ICommand EditMembershipCommand { get; }
        public ICommand DeleteMembershipCommand { get; }
        public ICommand BackCommand { get; }

        private async void LoadMemberships()
        {
            try
            {
                var memberships = await _dbContext.GetMembershipsAsync();
                Memberships.Clear();
                foreach (var membership in memberships)
                {
                    Memberships.Add(membership);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading memberships: {ex.Message}");
            }
        }

        private void CreateMembership(object? parameter)
        {
            var createViewModel = new MembershipCreateViewModel();
            createViewModel.MembershipCreated += OnMembershipCreated;
            createViewModel.CancelRequested += OnCancelRequested;
            CurrentView = createViewModel;
        }

        private bool CanEditMembership(object? parameter)
        {
            return SelectedMembership != null;
        }

        private void EditMembership(object? parameter)
        {
            if (SelectedMembership != null)
            {
                var editViewModel = new MembershipEditViewModel(SelectedMembership);
                editViewModel.MembershipUpdated += OnMembershipUpdated;
                editViewModel.CancelRequested += OnCancelRequested;
                CurrentView = editViewModel;
            }
        }

        private bool CanDeleteMembership(object? parameter)
        {
            return SelectedMembership != null;
        }

        private async void DeleteMembership(object? parameter)
        {
            if (SelectedMembership != null)
            {
                try
                {
                    await _dbContext.DeleteMembershipAsync(SelectedMembership.Id);
                    LoadMemberships();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting membership: {ex.Message}");
                }
            }
        }

        private void Back(object? parameter)
        {
            CurrentView = null;
        }

        private void OnMembershipCreated()
        {
            LoadMemberships();
            CurrentView = null;
        }

        private void OnMembershipUpdated()
        {
            LoadMemberships();
            CurrentView = null;
        }

        private void OnCancelRequested()
        {
            CurrentView = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}