using System;
using System.ComponentModel;

namespace GymApp.Models
{
    public class Membership : INotifyPropertyChanged
    {
        private int _id;
        private string _packageName = string.Empty;
        private string _description = string.Empty;
        private int _durationDays;
        private decimal _price;
        private bool _isActive;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
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

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}