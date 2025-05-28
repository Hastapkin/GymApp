using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace GymApp.ViewModels.Staff
{
    public class StaffListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Staff> _staffList;
        private Models.Staff _selectedStaff;
        private string _searchText;

        public ObservableCollection<Models.Staff> StaffList
        {
            get => _staffList;
            set { _staffList = value; OnPropertyChanged(nameof(StaffList)); }
        }

        public Models.Staff SelectedStaff
        {
            get => _selectedStaff;
            set { _selectedStaff = value; OnPropertyChanged(nameof(SelectedStaff)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public StaffListViewModel()
        {
            _dbContext = new DbContext();
            StaffList = new ObservableCollection<Models.Staff>();

            LoadDataCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchStaff);
            DeleteCommand = new RelayCommand(DeleteStaff, () => SelectedStaff != null);
            RefreshCommand = new RelayCommand(LoadData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                StaffList.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"SELECT Id, FullName, Phone, Email, Role, StartDate, Salary, 
                              Address, IsActive, Notes, CreatedDate 
                              FROM Staff ORDER BY Id DESC";

                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var staff = new Models.Staff
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Role = reader["Role"]?.ToString(),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        Salary = Convert.ToDecimal(reader["Salary"]),
                        Address = reader["Address"]?.ToString(),
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        Notes = reader["Notes"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                    };
                    StaffList.Add(staff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchStaff()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            try
            {
                StaffList.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"SELECT Id, FullName, Phone, Email, Role, StartDate, Salary, 
                              Address, IsActive, Notes, CreatedDate 
                              FROM Staff 
                              WHERE UPPER(FullName) LIKE UPPER(:searchText) 
                              OR UPPER(Phone) LIKE UPPER(:searchText)
                              OR UPPER(Email) LIKE UPPER(:searchText)
                              OR UPPER(Role) LIKE UPPER(:searchText)
                              ORDER BY Id DESC";

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":searchText", $"%{SearchText}%");
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var staff = new Models.Staff
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Role = reader["Role"]?.ToString(),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        Salary = Convert.ToDecimal(reader["Salary"]),
                        Address = reader["Address"]?.ToString(),
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        Notes = reader["Notes"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                    };
                    StaffList.Add(staff);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteStaff()
        {
            if (SelectedStaff == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên '{SelectedStaff.FullName}'?\n" +
                                       $"Chức vụ: {SelectedStaff.Role}\n" +
                                       $"Lương: {SelectedStaff.Salary:N0} VNĐ",
                                       "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    string sql = "DELETE FROM Staff WHERE Id = :id";
                    using var cmd = new OracleCommand(sql, connection);
                    cmd.Parameters.Add(":id", SelectedStaff.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        StaffList.Remove(SelectedStaff);
                        MessageBox.Show("Xóa nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}