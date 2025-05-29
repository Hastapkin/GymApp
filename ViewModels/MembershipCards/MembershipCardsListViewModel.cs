using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace GymApp.ViewModels.MembershipCards
{
    public class MembershipCardsListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.MembershipCards> _membershipCards;
        private Models.MembershipCards _selectedMembershipCard;
        private string _searchText;

        public ObservableCollection<Models.MembershipCards> MembershipCards
        {
            get => _membershipCards;
            set { _membershipCards = value; OnPropertyChanged(nameof(MembershipCards)); }
        }

        public Models.MembershipCards SelectedMembershipCard
        {
            get => _selectedMembershipCard;
            set { _selectedMembershipCard = value; OnPropertyChanged(nameof(SelectedMembershipCard)); }
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

        public MembershipCardsListViewModel()
        {
            _dbContext = new DbContext();
            MembershipCards = new ObservableCollection<Models.MembershipCards>();

            LoadDataCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchMembershipCards);
            DeleteCommand = new RelayCommand(DeleteMembershipCard, () => SelectedMembershipCard != null);
            RefreshCommand = new RelayCommand(LoadData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                MembershipCards.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                // FIX: ORDER BY mc.Id ASC để thẻ tập mới ở cuối
                string sql = @"SELECT mc.Id, mc.MemberId, mc.PackageId, mc.StartDate, mc.EndDate, 
                              mc.Price, mc.PaymentMethod, mc.Status, mc.Notes, mc.CreatedDate, mc.CreatedBy,
                              m.FullName as MemberName, p.PackageName
                              FROM MembershipCards mc
                              LEFT JOIN Members m ON mc.MemberId = m.Id
                              LEFT JOIN Packages p ON mc.PackageId = p.Id
                              ORDER BY mc.Id ASC"; // ← QUAN TRỌNG: mc.Id tăng dần

                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var membershipCard = new Models.MembershipCards
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        MemberId = Convert.ToInt32(reader["MemberId"]),
                        PackageId = Convert.ToInt32(reader["PackageId"]),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        Price = Convert.ToDecimal(reader["Price"]),
                        PaymentMethod = reader["PaymentMethod"]?.ToString(),
                        Status = reader["Status"]?.ToString(),
                        Notes = reader["Notes"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                        CreatedBy = reader["CreatedBy"]?.ToString(),
                        MemberName = reader["MemberName"]?.ToString(),
                        PackageName = reader["PackageName"]?.ToString()
                    };
                    MembershipCards.Add(membershipCard);
                }

                // Debug để xác nhận thứ tự
                System.Diagnostics.Debug.WriteLine($"MembershipCards loaded in order:");
                for (int i = 0; i < MembershipCards.Count; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"Position {i}: ID={MembershipCards[i].Id}, Member={MembershipCards[i].MemberName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchMembershipCards()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
                return;
            }

            try
            {
                MembershipCards.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                // FIX: ORDER BY mc.Id ASC trong search
                string sql = @"SELECT mc.Id, mc.MemberId, mc.PackageId, mc.StartDate, mc.EndDate, 
                              mc.Price, mc.PaymentMethod, mc.Status, mc.Notes, mc.CreatedDate, mc.CreatedBy,
                              m.FullName as MemberName, p.PackageName
                              FROM MembershipCards mc
                              LEFT JOIN Members m ON mc.MemberId = m.Id
                              LEFT JOIN Packages p ON mc.PackageId = p.Id
                              WHERE UPPER(m.FullName) LIKE UPPER(:searchText) 
                              OR UPPER(p.PackageName) LIKE UPPER(:searchText)
                              OR UPPER(mc.Status) LIKE UPPER(:searchText)
                              ORDER BY mc.Id ASC"; // ← QUAN TRỌNG: Giữ thứ tự mc.Id tăng dần

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":searchText", $"%{SearchText}%");
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var membershipCard = new Models.MembershipCards
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        MemberId = Convert.ToInt32(reader["MemberId"]),
                        PackageId = Convert.ToInt32(reader["PackageId"]),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        Price = Convert.ToDecimal(reader["Price"]),
                        PaymentMethod = reader["PaymentMethod"]?.ToString(),
                        Status = reader["Status"]?.ToString(),
                        Notes = reader["Notes"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                        CreatedBy = reader["CreatedBy"]?.ToString(),
                        MemberName = reader["MemberName"]?.ToString(),
                        PackageName = reader["PackageName"]?.ToString()
                    };
                    MembershipCards.Add(membershipCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMembershipCard()
        {
            if (SelectedMembershipCard == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thẻ tập của '{SelectedMembershipCard.MemberName}'?\n" +
                                       $"Gói: {SelectedMembershipCard.PackageName}\n" +
                                       $"Thời gian: {SelectedMembershipCard.StartDate:dd/MM/yyyy} - {SelectedMembershipCard.EndDate:dd/MM/yyyy}",
                                       "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    string sql = "DELETE FROM MembershipCards WHERE Id = :id";
                    using var cmd = new OracleCommand(sql, connection);
                    cmd.Parameters.Add(":id", SelectedMembershipCard.Id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MembershipCards.Remove(SelectedMembershipCard);
                        MessageBox.Show("Xóa thẻ tập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa thẻ tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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