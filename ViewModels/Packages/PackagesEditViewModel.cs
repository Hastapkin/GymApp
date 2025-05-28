using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace GymApp.ViewModels.Packages
{
    public class PackagesEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private Models.Packages _package;

        public Models.Packages Package
        {
            get => _package;
            set { _package = value; OnPropertyChanged(nameof(Package)); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public PackagesEditViewModel(Models.Packages package)
        {
            _dbContext = new DbContext();
            Package = new Models.Packages
            {
                Id = package.Id,
                PackageName = package.PackageName,
                Description = package.Description,
                DurationDays = package.DurationDays,
                Price = package.Price,
                IsActive = package.IsActive,
                CreatedDate = package.CreatedDate
            };

            SaveCommand = new RelayCommand(SavePackage);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SavePackage()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(Package.PackageName))
                {
                    MessageBox.Show("Vui lòng nhập tên gói tập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Package.DurationDays <= 0)
                {
                    MessageBox.Show("Số ngày phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Package.Price <= 0)
                {
                    MessageBox.Show("Giá phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"UPDATE Packages SET PackageName = :packageName, Description = :description, 
                              DurationDays = :durationDays, Price = :price, IsActive = :isActive 
                              WHERE Id = :id";

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":packageName", Package.PackageName);
                cmd.Parameters.Add(":description", Package.Description ?? (object)DBNull.Value);
                cmd.Parameters.Add(":durationDays", Package.DurationDays);
                cmd.Parameters.Add(":price", Package.Price);
                cmd.Parameters.Add(":isActive", Package.IsActive ? 1 : 0);
                cmd.Parameters.Add(":id", Package.Id);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Cập nhật gói tập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật gói tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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