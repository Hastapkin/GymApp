using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace GymApp.ViewModels.Packages
{
    public class PackagesListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Packages> _packages;
        private Models.Packages _selectedPackage;
        private string _searchText;

        public ObservableCollection<Models.Packages> Packages
        {
            get => _packages;
            set { _packages = value; OnPropertyChanged(nameof(Packages)); }
        }

        public Models.Packages SelectedPackage
        {
            get => _selectedPackage;
            set { _selectedPackage = value; OnPropertyChanged(nameof(SelectedPackage)); }
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

        public PackagesListViewModel()
        {
            _dbContext = new DbContext();
            Packages = new ObservableCollection<Models.Packages>();

            LoadDataCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchPackages);
            DeleteCommand = new RelayCommand(DeletePackage, () => SelectedPackage != null);
            RefreshCommand = new RelayCommand(LoadData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Packages.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                // FIX: ORDER BY Id ASC để package mới (ID lớn hơn) xuất hiện ở cuối
                string sql = @"SELECT Id, PackageName, Description, DurationDays, Price, IsActive, CreatedDate 
                              FROM Packages 
                              ORDER BY Id ASC"; // ← QUAN TRỌNG: Id tăng dần = package mới ở cuối

                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var package = new Models.Packages
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        PackageName = reader["PackageName"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        DurationDays = Convert.ToInt32(reader["DurationDays"]),
                        Price = Convert.ToDecimal(reader["Price"]),
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                    };

                    Packages.Add(package); // Add theo thứ tự Id tăng dần
                }

                // Debug để xác nhận thứ tự
                System.Diagnostics.Debug.WriteLine($"Packages loaded in order:");
                for (int i = 0; i < Packages.Count; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"Position {i}: ID={Packages[i].Id}, Name={Packages[i].PackageName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchPackages()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            try
            {
                Packages.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                // FIX: ORDER BY Id ASC trong search cũng vậy
                string sql = @"SELECT Id, PackageName, Description, DurationDays, Price, IsActive, CreatedDate 
                              FROM Packages 
                              WHERE UPPER(PackageName) LIKE UPPER(:searchText) 
                              OR UPPER(Description) LIKE UPPER(:searchText)
                              ORDER BY Id ASC"; // ← QUAN TRỌNG: Giữ thứ tự Id tăng dần

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":searchText", $"%{SearchText}%");
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var package = new Models.Packages
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        PackageName = reader["PackageName"]?.ToString(),
                        Description = reader["Description"]?.ToString(),
                        DurationDays = Convert.ToInt32(reader["DurationDays"]),
                        Price = Convert.ToDecimal(reader["Price"]),
                        IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                    };
                    Packages.Add(package);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePackage()
        {
            if (SelectedPackage == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa gói tập '{SelectedPackage.PackageName}'?\n" +
                                       "Lưu ý: Việc xóa gói tập có thể ảnh hưởng đến các thẻ tập đang sử dụng gói này.",
                                       "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    // Check if package is being used by any membership cards
                    string checkSql = "SELECT COUNT(*) FROM MembershipCards WHERE PackageId = :id";
                    using var checkCmd = new OracleCommand(checkSql, connection);
                    checkCmd.Parameters.Add(":id", SelectedPackage.Id);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show($"Không thể xóa gói tập này vì đang có {count} thẻ tập sử dụng.\n" +
                                      "Vui lòng xóa hoặc chuyển các thẻ tập sang gói khác trước khi xóa gói này.",
                                      "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string sql = "DELETE FROM Packages WHERE Id = :id";
                    using var cmd = new OracleCommand(sql, connection);
                    cmd.Parameters.Add(":id", SelectedPackage.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Packages.Remove(SelectedPackage);
                        MessageBox.Show("Xóa gói tập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa gói tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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