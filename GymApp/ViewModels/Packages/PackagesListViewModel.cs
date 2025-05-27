using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;

namespace GymApp.ViewModels.Packages
{
    public class PackagesListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Packages> _packages;
        private Models.Packages? _selectedPackage;
        private object? _currentView;

        public PackagesListViewModel()
        {
            _dbContext = new DbContext();
            _packages = new ObservableCollection<Models.Packages>();

            CreatePackageCommand = new RelayCommand(CreatePackage);
            EditPackageCommand = new RelayCommand(EditPackage, CanEditPackage);
            DeletePackageCommand = new RelayCommand(DeletePackage, CanDeletePackage);

            LoadPackages();
        }

        public ObservableCollection<Models.Packages> Packages
        {
            get => _packages;
            set { _packages = value; OnPropertyChanged(nameof(Packages)); }
        }

        public Models.Packages? SelectedPackage
        {
            get => _selectedPackage;
            set { _selectedPackage = value; OnPropertyChanged(nameof(SelectedPackage)); }
        }

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand CreatePackageCommand { get; }
        public ICommand EditPackageCommand { get; }
        public ICommand DeletePackageCommand { get; }

        private async void LoadPackages()
        {
            try
            {
                var packages = await _dbContext.GetPackagesAsync();
                Packages.Clear();
                foreach (var package in packages)
                    Packages.Add(package);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading packages: {ex.Message}");
            }
        }

        private void CreatePackage(object? parameter)
        {
            var createViewModel = new PackagesCreateViewModel();
            createViewModel.PackageCreated += () => { LoadPackages(); CurrentView = null; };
            createViewModel.CancelRequested += () => CurrentView = null;
            CurrentView = createViewModel;
        }

        private bool CanEditPackage(object? parameter) => SelectedPackage != null;

        private void EditPackage(object? parameter)
        {
            if (SelectedPackage != null)
            {
                var editViewModel = new PackagesEditViewModel(SelectedPackage);
                editViewModel.PackageUpdated += () => { LoadPackages(); CurrentView = null; };
                editViewModel.CancelRequested += () => CurrentView = null;
                CurrentView = editViewModel;
            }
        }

        private bool CanDeletePackage(object? parameter) => SelectedPackage != null;

        private async void DeletePackage(object? parameter)
        {
            if (SelectedPackage != null)
            {
                try
                {
                    await _dbContext.DeletePackageAsync(SelectedPackage.Id);
                    LoadPackages();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting package: {ex.Message}");
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