using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;
using GymApp.Views.Members;

namespace GymApp.ViewModels.Member
{
    public class MemberListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Member> _members;
        private Models.Member? _selectedMember;
        private object? _currentView;

        public MemberListViewModel()
        {
            _dbContext = new DbContext();
            _members = new ObservableCollection<Models.Member>();

            CreateMemberCommand = new RelayCommand(CreateMember);
            EditMemberCommand = new RelayCommand(EditMember, CanEditMember);
            DeleteMemberCommand = new RelayCommand(DeleteMember, CanDeleteMember);
            BackCommand = new RelayCommand(Back);

            LoadMembers();
        }

        public ObservableCollection<Models.Member> Members
        {
            get => _members;
            set
            {
                _members = value;
                OnPropertyChanged(nameof(Members));
            }
        }

        public Models.Member? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
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

        public ICommand CreateMemberCommand { get; }
        public ICommand EditMemberCommand { get; }
        public ICommand DeleteMemberCommand { get; }
        public ICommand BackCommand { get; }

        private async void LoadMembers()
        {
            try
            {
                var members = await _dbContext.GetMembersAsync();
                Members.Clear();
                foreach (var member in members)
                {
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                // Handle error - in a real app, you might show a message box or log the error
                System.Diagnostics.Debug.WriteLine($"Error loading members: {ex.Message}");
            }
        }

        private void CreateMember(object? parameter)
        {
            var createViewModel = new MemberCreateViewModel();
            createViewModel.MemberCreated += OnMemberCreated;
            createViewModel.CancelRequested += OnCancelRequested;

            var createView = new MemberCreateView();
            createView.DataContext = createViewModel;
            CurrentView = createView;
        }

        private bool CanEditMember(object? parameter)
        {
            return SelectedMember != null;
        }

        private void EditMember(object? parameter)
        {
            if (SelectedMember != null)
            {
                var editViewModel = new MemberEditViewModel(SelectedMember);
                editViewModel.MemberUpdated += OnMemberUpdated;
                editViewModel.CancelRequested += OnCancelRequested;

                var editView = new MemberEditView();
                editView.DataContext = editViewModel;
                CurrentView = editView;
            }
        }

        private bool CanDeleteMember(object? parameter)
        {
            return SelectedMember != null;
        }

        private async void DeleteMember(object? parameter)
        {
            if (SelectedMember != null)
            {
                try
                {
                    await _dbContext.DeleteMemberAsync(SelectedMember.Id);
                    LoadMembers();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting member: {ex.Message}");
                }
            }
        }

        private void Back(object? parameter)
        {
            CurrentView = null;
        }

        private void OnMemberCreated()
        {
            LoadMembers();
            CurrentView = null;
        }

        private void OnMemberUpdated()
        {
            LoadMembers();
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