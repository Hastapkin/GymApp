using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;

namespace GymApp.ViewModels.Membership
{
    public class MembershipEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private readonly int _membershipId;
        private string _packageName = string.Empty;
        private string _description = string.Empty;
        private int _durationDays = 30;
        private decimal _price = 0;

        public MembershipEditViewModel(Models.Membership membership)
        {
            _dbContext = new DbContext();
            _membershipId = membership.Id;

            // Initialize with existing membership data
            PackageName = membership.PackageName;
            Description = membership.Description;
            DurationDays = membership.DurationDays;
            Price = membership.Price;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string PackageName
        {
            get => _packageName;
            set
            {
                _packageName = value;
                OnPropertyChanged(nameof(PackageName));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public int DurationDays
        {
            get => _durationDays;
            set
            {
                _durationDays = value;
                OnPropertyChanged(nameof(DurationDays));
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? MembershipUpdated;
        public event Action? CancelRequested;

        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(PackageName) && DurationDays > 0 && Price >= 0;
        }

        private async void Save(object? parameter)
        {
            try
            {
                var membership = new Models.Membership
                {
                    Id = _membershipId,
                    PackageName = PackageName,
                    Description = Description,
                    DurationDays = DurationDays,
                    Price = Price,
                    IsActive = true
                };

                await _dbContext.UpdateMembershipAsync(membership);
                MembershipUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating membership: {ex.Message}");
            }
        }

        private void Cancel(object? parameter)
        {
            CancelRequested?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}