using System;
using System.ComponentModel;

namespace GymApp.Models
{
    public class Staff : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public decimal Salary { get; set; }
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}