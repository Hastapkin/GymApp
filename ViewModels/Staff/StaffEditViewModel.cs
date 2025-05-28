using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System.Collections.Generic;

namespace GymApp.ViewModels.Staff
{
    public class StaffEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private Models.Staff _staff;

        public Models.Staff Staff
        {
            get => _staff;
            set { _staff = value; OnPropertyChanged(nameof(Staff)); }
        }

        public List<string> RoleOptions { get; } = new List<string>
        {
            "Lao công",
            "Thu ngân",
            "Quản lý",
            "Huấn luyện viên"
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public StaffEditViewModel(Models.Staff staff)
        {
            _dbContext = new DbContext();
            Staff = new Models.Staff
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Phone = staff.Phone,
                Email = staff.Email,
                Role = staff.Role,
                StartDate = staff.StartDate,
                Salary = staff.Salary,
                Address = staff.Address,
                IsActive = staff.IsActive,
                Notes = staff.Notes,
                CreatedDate = staff.CreatedDate
            };

            SaveCommand = new RelayCommand(SaveStaff);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveStaff()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(Staff.FullName))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Staff.Role))
                {
                    MessageBox.Show("Vui lòng chọn chức vụ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Staff.Salary < 0)
                {
                    MessageBox.Show("Lương không được âm!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate email format if provided
                if (!string.IsNullOrWhiteSpace(Staff.Email) && !IsValidEmail(Staff.Email))
                {
                    MessageBox.Show("Email không đúng định dạng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"UPDATE Staff SET FullName = :fullName, Phone = :phone, Email = :email, 
                              Role = :role, StartDate = :startDate, Salary = :salary, 
                              Address = :address, IsActive = :isActive, Notes = :notes 
                              WHERE Id = :id";

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":fullName", Staff.FullName);
                cmd.Parameters.Add(":phone", Staff.Phone ?? (object)DBNull.Value);
                cmd.Parameters.Add(":email", Staff.Email ?? (object)DBNull.Value);
                cmd.Parameters.Add(":role", Staff.Role);
                cmd.Parameters.Add(":startDate", Staff.StartDate);
                cmd.Parameters.Add(":salary", Staff.Salary);
                cmd.Parameters.Add(":address", Staff.Address ?? (object)DBNull.Value);
                cmd.Parameters.Add(":isActive", Staff.IsActive ? 1 : 0);
                cmd.Parameters.Add(":notes", Staff.Notes ?? (object)DBNull.Value);
                cmd.Parameters.Add(":id", Staff.Id);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}