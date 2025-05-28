using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System.Collections.Generic;

namespace GymApp.ViewModels.Member
{
    public class MemberEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private Models.Member _member;

        public Models.Member Member
        {
            get => _member;
            set { _member = value; OnPropertyChanged(nameof(Member)); }
        }

        public List<string> GenderOptions { get; } = new List<string> { "Nam", "Nữ", "Khác" };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public MemberEditViewModel(Models.Member member)
        {
            _dbContext = new DbContext();
            Member = new Models.Member
            {
                Id = member.Id,
                FullName = member.FullName,
                Phone = member.Phone,
                Email = member.Email,
                Gender = member.Gender,
                DateOfBirth = member.DateOfBirth,
                Address = member.Address,
                JoinDate = member.JoinDate,
                IsActive = member.IsActive,
                Notes = member.Notes,
                CreatedDate = member.CreatedDate,
                UpdatedDate = member.UpdatedDate
            };

            SaveCommand = new RelayCommand(SaveMember);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveMember()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(Member.FullName))
                {
                    MessageBox.Show("Vui lòng nhập họ tên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"UPDATE Members SET FullName = :fullName, Phone = :phone, Email = :email, 
                              Gender = :gender, DateOfBirth = :dateOfBirth, Address = :address, 
                              JoinDate = :joinDate, IsActive = :isActive, Notes = :notes, 
                              UpdatedDate = :updatedDate 
                              WHERE Id = :id";

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":fullName", Member.FullName);
                cmd.Parameters.Add(":phone", Member.Phone ?? (object)DBNull.Value);
                cmd.Parameters.Add(":email", Member.Email ?? (object)DBNull.Value);
                cmd.Parameters.Add(":gender", Member.Gender ?? (object)DBNull.Value);
                cmd.Parameters.Add(":dateOfBirth", Member.DateOfBirth ?? (object)DBNull.Value);
                cmd.Parameters.Add(":address", Member.Address ?? (object)DBNull.Value);
                cmd.Parameters.Add(":joinDate", Member.JoinDate);
                cmd.Parameters.Add(":isActive", Member.IsActive ? 1 : 0);
                cmd.Parameters.Add(":notes", Member.Notes ?? (object)DBNull.Value);
                cmd.Parameters.Add(":updatedDate", DateTime.Now);
                cmd.Parameters.Add(":id", Member.Id);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Cập nhật thành viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật thành viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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