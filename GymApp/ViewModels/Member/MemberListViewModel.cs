using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;

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

            LoadMembers();
        }

        public ObservableCollection<Models.Member> Members
        {
            get => _members;
            set { _members = value; OnPropertyChanged(nameof(Members)); }
        }

        public Models.Member? SelectedMember
        {
            get => _selectedMember;
            set { _selectedMember = value; OnPropertyChanged(nameof(SelectedMember)); }
        }

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand CreateMemberCommand { get; }
        public ICommand EditMemberCommand { get; }
        public ICommand DeleteMemberCommand { get; }

        private async void LoadMembers()
        {
            try
            {
                var members = await _dbContext.GetMembersAsync();
                Members.Clear();
                foreach (var member in members)
                    Members.Add(member);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading members: {ex.Message}");
            }
        }

        private void CreateMember(object? parameter)
        {
            var createViewModel = new MemberCreateViewModel();
            createViewModel.MemberCreated += () => { LoadMembers(); CurrentView = null; };
            createViewModel.CancelRequested += () => CurrentView = null;
            CurrentView = createViewModel;
        }

        private bool CanEditMember(object? parameter) => SelectedMember != null;

        private void EditMember(object? parameter)
        {
            if (SelectedMember != null)
            {
                var editViewModel = new MemberEditViewModel(SelectedMember);
                editViewModel.MemberUpdated += () => { LoadMembers(); CurrentView = null; };
                editViewModel.CancelRequested += () => CurrentView = null;
                CurrentView = editViewModel;
            }
        }

        private bool CanDeleteMember(object? parameter) => SelectedMember != null;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}