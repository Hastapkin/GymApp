using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;

namespace GymApp.ViewModels.Membership
{
    public class MembershipCardsEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private readonly int _membershipCardId;
        private int _memberId;
        private int _packageId;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _price;
        private string _paymentMethod = "Tiền mặt";
        private string _status = "Hoạt động";
        private string _notes = string.Empty;

        private ObservableCollection<Models.Member> _members;
        private ObservableCollection<Models.Packages> _packages;
        private Models.Member? _selectedMember;
        private Models.Packages? _selectedPackage;

        public MembershipCardsEditViewModel(MembershipCards membershipCard)
        {
            _dbContext = new DbContext();
            _membershipCardId = membershipCard.Id;
            _members = new ObservableCollection<Models.Member>();
            _packages = new ObservableCollection<Models.Packages>();

            // Initialize with existing membership card data
            MemberId = membershipCard.MemberId;
            PackageId = membershipCard.PackageId;
            StartDate = membershipCard.StartDate;
            EndDate = membershipCard.EndDate;
            Price = membershipCard.Price;
            PaymentMethod = membershipCard.PaymentMethod;
            Status = membershipCard.Status;
            Notes = membershipCard.Notes;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            PaymentMethods = new[] { "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng", "Ví điện tử" };
            Statuses = new[] { "Hoạt động", "Tạm ngưng", "Hết hạn" };

            LoadMembers();
            LoadPackages();
        }

        public int MemberId
        {
            get => _memberId;
            set { _memberId = value; OnPropertyChanged(nameof(MemberId)); }
        }

        public int PackageId
        {
            get => _packageId;
            set { _packageId = value; OnPropertyChanged(nameof(PackageId)); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); CalculateEndDate(); }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(nameof(Price)); }
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        public ObservableCollection<Models.Member> Members
        {
            get => _members;
            set { _members = value; OnPropertyChanged(nameof(Members)); }
        }

        public ObservableCollection<Models.Packages> Packages
        {
            get => _packages;
            set { _packages = value; OnPropertyChanged(nameof(Packages)); }
        }

        public Models.Member? SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                if (value != null) MemberId = value.Id;
                OnPropertyChanged(nameof(SelectedMember));
            }
        }

        public Models.Packages? SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                _selectedPackage = value;
                if (value != null)
                {
                    PackageId = value.Id;
                    Price = value.Price;
                    CalculateEndDate();
                }
                OnPropertyChanged(nameof(SelectedPackage));
            }
        }

        public string[] PaymentMethods { get; }
        public string[] Statuses { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? MembershipCardUpdated;
        public event Action? CancelRequested;

        private async void LoadMembers()
        {
            try
            {
                var members = await _dbContext.GetMembersAsync();
                Members.Clear();
                foreach (var member in members.Where(m => m.IsActive))
                    Members.Add(member);

                // Set selected member based on current MemberId
                SelectedMember = Members.FirstOrDefault(m => m.Id == MemberId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading members: {ex.Message}");
            }
        }

        private async void LoadPackages()
        {
            try
            {
                var packages = await _dbContext.GetPackagesAsync();
                Packages.Clear();
                foreach (var package in packages.Where(p => p.IsActive))
                    Packages.Add(package);

                // Set selected package based on current PackageId
                SelectedPackage = Packages.FirstOrDefault(p => p.Id == PackageId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading packages: {ex.Message}");
            }
        }

        private void CalculateEndDate()
        {
            if (SelectedPackage != null)
                EndDate = StartDate.AddDays(SelectedPackage.DurationDays);
        }

        private bool CanSave(object? parameter) => MemberId > 0 && PackageId > 0 && Price > 0;

        private async void Save(object? parameter)
        {
            try
            {
                var membershipCard = new MembershipCards
                {
                    Id = _membershipCardId,
                    MemberId = MemberId,
                    PackageId = PackageId,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Price = Price,
                    PaymentMethod = PaymentMethod,
                    Status = Status,
                    Notes = Notes
                };

                await _dbContext.UpdateMembershipCardAsync(membershipCard);
                MembershipCardUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating membership card: {ex.Message}");
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