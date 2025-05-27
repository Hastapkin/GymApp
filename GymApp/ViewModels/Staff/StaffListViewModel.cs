using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;

namespace GymApp.ViewModels.Staff
{
    public class StaffListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Staff> _staff;
        private Models.Staff? _selectedStaff;
        private object? _currentView;

        public StaffListViewModel()
        {
            _dbContext = new DbContext();
            _staff = new ObservableCollection<Models.Staff>();

            CreateStaffCommand = new RelayCommand(CreateStaff);
            EditStaffCommand = new RelayCommand(EditStaff, CanEditStaff);
            DeleteStaffCommand = new RelayCommand(DeleteStaff, CanDeleteStaff);

            LoadStaff();
        }

        public ObservableCollection<Models.Staff> Staff
        {
            get => _staff;
            set { _staff = value; OnPropertyChanged(nameof(Staff)); }
        }

        public Models.Staff? SelectedStaff
        {
            get => _selectedStaff;
            set { _selectedStaff = value; OnPropertyChanged(nameof(SelectedStaff)); }
        }

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand CreateStaffCommand { get; }
        public ICommand EditStaffCommand { get; }
        public ICommand DeleteStaffCommand { get; }

        private async void LoadStaff()
        {
            try
            {
                var staffList = await _dbContext.GetStaffAsync();
                Staff.Clear();
                foreach (var staff in staffList)
                    Staff.Add(staff);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading staff: {ex.Message}");
            }
        }

        private void CreateStaff(object? parameter)
        {
            var createViewModel = new StaffCreateViewModel();
            createViewModel.StaffCreated += () => { LoadStaff(); CurrentView = null; };
            createViewModel.CancelRequested += () => CurrentView = null;
            CurrentView = createViewModel;
        }

        private bool CanEditStaff(object? parameter) => SelectedStaff != null;

        private void EditStaff(object? parameter)
        {
            if (SelectedStaff != null)
            {
                var editViewModel = new StaffEditViewModel(SelectedStaff);
                editViewModel.StaffUpdated += () => { LoadStaff(); CurrentView = null; };
                editViewModel.CancelRequested += () => CurrentView = null;
                CurrentView = editViewModel;
            }
        }

        private bool CanDeleteStaff(object? parameter) => SelectedStaff != null;

        private async void DeleteStaff(object? parameter)
        {
            if (SelectedStaff != null)
            {
                try
                {
                    await _dbContext.DeleteStaffAsync(SelectedStaff.Id);
                    LoadStaff();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting staff: {ex.Message}");
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