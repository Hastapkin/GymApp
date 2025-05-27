using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;

namespace GymApp.ViewModels.Packages
{
    public class PackagesEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private readonly int _packageId;
        private string _packageName = string.Empty;
        private string _description = string.Empty;
        private int _durationDays = 30;
        private decimal _price = 0;

        public PackagesEditViewModel(Models.Packages package)
        {
            _dbContext = new DbContext();
            _packageId = package.Id;

            PackageName = package.PackageName;
            Description = package.Description;
            DurationDays = package.DurationDays;
            Price = package.Price;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string PackageName
        {
            get => _packageName;
            set { _packageName = value; OnPropertyChanged(nameof(PackageName)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public int DurationDays
        {
            get => _durationDays;
            set { _durationDays = value; OnPropertyChanged(nameof(DurationDays)); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(nameof(Price)); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? PackageUpdated;
        public event Action? CancelRequested;

        private bool CanSave(object? parameter) => !string.IsNullOrWhiteSpace(PackageName) && DurationDays > 0 && Price >= 0;

        private async void Save(object? parameter)
        {
            try
            {
                var package = new Models.Packages
                {
                    Id = _packageId,
                    PackageName = PackageName,
                    Description = Description,
                    DurationDays = DurationDays,
                    Price = Price,
                    IsActive = true
                };

                await _dbContext.UpdatePackageAsync(package);
                PackageUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating package: {ex.Message}");
            }
        }

        private void Cancel(object? parameter) => CancelRequested?.Invoke();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}