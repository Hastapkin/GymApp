using System;
using System.ComponentModel;

namespace GymApp.Models
{
    public class MembershipCard : INotifyPropertyChanged
    {
        private int _id;
        private int _memberId;
        private string _memberName;
        private int _packageId;
        private string _packageName;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _price;
        private string _paymentMethod;
        private string _status;
        private string _notes;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public int MemberId
        {
            get => _memberId;
            set { _memberId = value; OnPropertyChanged(nameof(MemberId)); }
        }

        public string MemberName
        {
            get => _memberName;
            set { _memberName = value; OnPropertyChanged(nameof(MemberName)); }
        }

        public int PackageId
        {
            get => _packageId;
            set { _packageId = value; OnPropertyChanged(nameof(PackageId)); }
        }

        public string PackageName
        {
            get => _packageName;
            set { _packageName = value; OnPropertyChanged(nameof(PackageName)); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
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

        public string StatusDisplay => EndDate >= DateTime.Now ? "Còn hạn" : "Hết hạn";
        public int DaysRemaining => (int)(EndDate - DateTime.Now).TotalDays;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Package
    {
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string Description { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}