using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace GymApp.ViewModels.Member
{
    public class MemberListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Member> _members;
        private Models.Member _selectedMember;
        private string _searchText;

        public ObservableCollection<Models.Member> Members
        {
            get => _members;
            set { _members = value; OnPropertyChanged(nameof(Members)); }
        }

        public Models.Member SelectedMember
        {
            get => _selectedMember;
            set { _selectedMember = value; OnPropertyChanged(nameof(SelectedMember)); }
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

        public MemberListViewModel()
        {
            _dbContext = new DbContext();
            Members = new ObservableCollection<Models.Member>();

            LoadDataCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchMembers);
            DeleteCommand = new RelayCommand(DeleteMember, () => SelectedMember != null);
            RefreshCommand = new RelayCommand(LoadData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Members.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"SELECT Id, FullName, Phone, Email, Gender, DateOfBirth, Address, 
                              JoinDate, IsActive, Notes, CreatedDate, UpdatedDate 
                              FROM Members ORDER BY Id DESC";

                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var member = new Models.Member
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Gender = reader["Gender"]?.ToString(),
                        DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]) : null,
                        Address = reader["Address"]?.ToString(),
                        JoinDate = Convert.ToDateTime(reader["JoinDate"]),
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        Notes = reader["Notes"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                        UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"])
                    };
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchMembers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            try
            {
                Members.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"SELECT Id, FullName, Phone, Email, Gender, DateOfBirth, Address, 
                              JoinDate, IsActive, Notes, CreatedDate, UpdatedDate 
                              FROM Members 
                              WHERE UPPER(FullName) LIKE UPPER(:searchText) 
                              OR UPPER(Phone) LIKE UPPER(:searchText)
                              OR UPPER(Email) LIKE UPPER(:searchText)
                              ORDER BY Id DESC";

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":searchText", $"%{SearchText}%");
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var member = new Models.Member
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Gender = reader["Gender"]?.ToString(),
                        DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateOfBirth"]) : null,
                        Address = reader["Address"]?.ToString(),
                        JoinDate = Convert.ToDateTime(reader["JoinDate"]),
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        Notes = reader["Notes"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                        UpdatedDate = Convert.ToDateTime(reader["UpdatedDate"])
                    };
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMember()
        {
            if (SelectedMember == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thành viên '{SelectedMember.FullName}'?",
                                       "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    string sql = "DELETE FROM Members WHERE Id = :id";
                    using var cmd = new OracleCommand(sql, connection);
                    cmd.Parameters.Add(":id", SelectedMember.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Members.Remove(SelectedMember);
                        MessageBox.Show("Xóa thành viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa thành viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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