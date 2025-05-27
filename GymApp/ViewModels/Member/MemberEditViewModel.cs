using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;

namespace GymApp.ViewModels.Member
{
    public class MemberEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private readonly int _memberId;
        private string _fullName = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;
        private string _gender = "Nam";
        private DateTime? _dateOfBirth;
        private string _address = string.Empty;
        private string _notes = string.Empty;

        public MemberEditViewModel(Models.Member member)
        {
            _dbContext = new DbContext();
            _memberId = member.Id;

            FullName = member.FullName;
            Phone = member.Phone;
            Email = member.Email;
            Gender = member.Gender;
            DateOfBirth = member.DateOfBirth;
            Address = member.Address;
            Notes = member.Notes;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            Genders = new[] { "Nam", "Nữ", "Khác" };
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(nameof(FullName)); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(nameof(Gender)); }
        }

        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(nameof(DateOfBirth)); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        public string[] Genders { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? MemberUpdated;
        public event Action? CancelRequested;

        private bool CanSave(object? parameter) => !string.IsNullOrWhiteSpace(FullName);

        private async void Save(object? parameter)
        {
            try
            {
                var member = new Models.Member
                {
                    Id = _memberId,
                    FullName = FullName,
                    Phone = Phone,
                    Email = Email,
                    Gender = Gender,
                    DateOfBirth = DateOfBirth,
                    Address = Address,
                    Notes = Notes,
                    IsActive = true
                };

                await _dbContext.UpdateMemberAsync(member);
                MemberUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating member: {ex.Message}");
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