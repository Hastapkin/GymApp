using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;

namespace GymApp.ViewModels.Members_Info
{
    public class Members_InfoListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<Models.Members_Info> _membersInfo;
        private Models.Members_Info _selectedMemberInfo;
        private string _searchText;
        private string _filterStatus;

        public ObservableCollection<Models.Members_Info> MembersInfo
        {
            get => _membersInfo;
            set { _membersInfo = value; OnPropertyChanged(nameof(MembersInfo)); }
        }

        public Models.Members_Info SelectedMemberInfo
        {
            get => _selectedMemberInfo;
            set { _selectedMemberInfo = value; OnPropertyChanged(nameof(SelectedMemberInfo)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public string FilterStatus
        {
            get => _filterStatus;
            set { _filterStatus = value; OnPropertyChanged(nameof(FilterStatus)); FilterData(); }
        }

        public ObservableCollection<string> FilterOptions { get; } = new ObservableCollection<string>
        {
            "Tất cả",
            "Còn hạn",
            "Hết hạn",
            "Sắp hết hạn" // < 7 days
        };

        public ICommand LoadDataCommand { get; }
        private ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExtendMembershipCommand { get; }
        public ICommand CheckInCommand { get; }

        private ObservableCollection<Models.Members_Info> _allMembersInfo;

        public Members_InfoListViewModel()
        {
            _dbContext = new DbContext();
            MembersInfo = new ObservableCollection<Models.Members_Info>();
            _allMembersInfo = new ObservableCollection<Models.Members_Info>();
            FilterStatus = "Tất cả";

            LoadDataCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchMembers);
            RefreshCommand = new RelayCommand(LoadData);
            ExtendMembershipCommand = new RelayCommand(ExtendMembership, () => SelectedMemberInfo != null);
            CheckInCommand = new RelayCommand(CheckInMember, () => SelectedMemberInfo != null);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _allMembersInfo.Clear();
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"SELECT Id, FullName, Phone, Email, Gender, JoinDate, StartDate, EndDate, 
                              PackageName, Price, Status, MembershipStatus, DaysRemaining 
                              FROM V_MemberInfo ORDER BY DaysRemaining ASC, FullName";

                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var memberInfo = new Models.Members_Info
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Gender = reader["Gender"]?.ToString(),
                        JoinDate = reader["JoinDate"] != DBNull.Value ? Convert.ToDateTime(reader["JoinDate"]) : DateTime.MinValue,
                        StartDate = reader["StartDate"] != DBNull.Value ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue,
                        EndDate = reader["EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue,
                        PackageName = reader["PackageName"]?.ToString(),
                        Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                        Status = reader["Status"]?.ToString(),
                        MembershipStatus = reader["MembershipStatus"]?.ToString(),
                        DaysRemaining = reader["DaysRemaining"] != DBNull.Value ? Convert.ToInt32(reader["DaysRemaining"]) : 0
                    };
                    _allMembersInfo.Add(memberInfo);
                }

                FilterData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchMembers()
        {
            FilterData();
        }

        private void FilterData()
        {
            MembersInfo.Clear();

            foreach (var memberInfo in _allMembersInfo)
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                    memberInfo.FullName?.ToUpper().Contains(SearchText.ToUpper()) == true ||
                    memberInfo.Phone?.ToUpper().Contains(SearchText.ToUpper()) == true ||
                    memberInfo.Email?.ToUpper().Contains(SearchText.ToUpper()) == true ||
                    memberInfo.PackageName?.ToUpper().Contains(SearchText.ToUpper()) == true;

                bool matchesFilter = FilterStatus == "Tất cả" ||
                    (FilterStatus == "Còn hạn" && memberInfo.MembershipStatus == "Còn hạn") ||
                    (FilterStatus == "Hết hạn" && memberInfo.MembershipStatus == "Hết hạn") ||
                    (FilterStatus == "Sắp hết hạn" && memberInfo.MembershipStatus == "Còn hạn" && memberInfo.DaysRemaining <= 7);

                if (matchesSearch && matchesFilter)
                {
                    MembersInfo.Add(memberInfo);
                }
            }
        }

        private void ExtendMembership()
        {
            if (SelectedMemberInfo == null) return;

            var extendWindow = new Views.Members_Info.Members_InfoEditView(SelectedMemberInfo);
            if (extendWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void CheckInMember()
        {
            if (SelectedMemberInfo == null) return;

            if (SelectedMemberInfo.MembershipStatus == "Hết hạn")
            {
                MessageBox.Show("Thành viên này đã hết hạn thẻ tập!\nVui lòng gia hạn thẻ trước khi check-in.",
                              "Thẻ hết hạn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Check-in cho thành viên: {SelectedMemberInfo.FullName}?\n" +
                                       $"Thẻ còn hiệu lực: {SelectedMemberInfo.DaysRemaining} ngày",
                                       "Xác nhận check-in", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    string sql = @"INSERT INTO CheckInLog (MemberId, CheckInTime, Notes) 
                                  VALUES (:memberId, SYSDATE, :notes)";

                    using var cmd = new OracleCommand(sql, connection);
                    cmd.Parameters.Add(":memberId", SelectedMemberInfo.Id);
                    cmd.Parameters.Add(":notes", $"Check-in từ Members Info - {SelectedMemberInfo.PackageName}");

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Check-in thành công!\nThành viên: {SelectedMemberInfo.FullName}\nThời gian: {DateTime.Now:HH:mm dd/MM/yyyy}",
                                      "Check-in thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi check-in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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