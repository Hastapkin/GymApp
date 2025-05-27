using System;
using System.ComponentModel;

namespace GymApp.Models
{
    public class MembershipCards : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int PackageId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt";
        public string Status { get; set; } = "Hoạt động";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = "Admin";

        // Navigation properties
        public string MemberName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}