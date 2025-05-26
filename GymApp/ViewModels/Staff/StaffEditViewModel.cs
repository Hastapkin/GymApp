using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;

namespace GymApp.ViewModels.Staff
{
    public class StaffEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private readonly int _staffId;
        private string _fullName = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _role = "Thu ngân";
        private DateTime _startDate = DateTime.Now;
        private decimal _salary = 0;
        private string _address = string.Empty;
        private string _notes = string.Empty;

        public StaffEditViewModel(Models.Staff staff)
        {
            _dbContext = new DbContext();
            _staffId = staff.Id;

            // Initialize with existing staff data
            FullName = staff.FullName;
            Phone = staff.Phone;
            Email = staff.Email;
            Role = staff.Role;
            StartDate = staff.StartDate;
            Salary = staff.Salary;
            Address = staff.Address;
            Notes = staff.Notes;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            Roles = new[] { "Lao công", "Thu ngân", "Quản lý", "Huấn luyện viên" };
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged(nameof(Role));
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public decimal Salary
        {
            get => _salary;
            set
            {
                _salary = value;
                OnPropertyChanged(nameof(Salary));
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public string[] Roles { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? StaffUpdated;
        public event Action? CancelRequested;

        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Role);
        }

        private async void Save(object? parameter)
        {
            try
            {
                var staff = new Models.Staff
                {
                    Id = _staffId,
                    FullName = FullName,
                    Phone = Phone,
                    Email = Email,
                    Role = Role,
                    StartDate = StartDate,
                    Salary = Salary,
                    Address = Address,
                    Notes = Notes,
                    IsActive = true
                };

                await _dbContext.UpdateStaffAsync(staff);
                StaffUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating staff: {ex.Message}");
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