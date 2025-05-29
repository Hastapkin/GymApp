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
            "Sắp hết hạn" // <= 7 days
        };

        public ICommand LoadDataCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExtendMembershipCommand { get; }
        public ICommand CheckInCommand { get; }

        private ObservableCollection<Models.Members_Info> _allMembersInfo;

        public Members_InfoListViewModel()
        {
            _dbContext = new DbContext();

            // IMPORTANT: Khởi tạo collections trước khi gán
            MembersInfo = new ObservableCollection<Models.Members_Info>();
            _allMembersInfo = new ObservableCollection<Models.Members_Info>();
            FilterOptions = new ObservableCollection<string>
            {
                "Tất cả",
                "Còn hạn",
                "Hết hạn",
                "Sắp hết hạn"
            };

            FilterStatus = "Tất cả"; // ✅ Set default value

            LoadDataCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchMembers);
            RefreshCommand = new RelayCommand(LoadData);
            ExtendMembershipCommand = new RelayCommand(ExtendMembership, () => SelectedMemberInfo != null);
            CheckInCommand = new RelayCommand(CheckInMember, () => SelectedMemberInfo != null);

            // Load data with try-catch
            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
                // Đảm bảo collections không null ngay cả khi lỗi
                if (MembersInfo == null)
                    MembersInfo = new ObservableCollection<Models.Members_Info>();
                if (_allMembersInfo == null)
                    _allMembersInfo = new ObservableCollection<Models.Members_Info>();
            }
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
                              FROM V_MemberInfo 
                              ORDER BY Id ASC, DaysRemaining ASC";

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

            // ENHANCED: Kiểm tra trạng thái thẻ tập chi tiết hơn
            if (SelectedMemberInfo.MembershipStatus == "Hết hạn")
            {
                var result = MessageBox.Show($"Thẻ tập của {SelectedMemberInfo.FullName} đã hết hạn!\n" +
                                           $"Hết hạn từ: {SelectedMemberInfo.EndDate:dd/MM/yyyy}\n\n" +
                                           "Bạn có muốn gia hạn thẻ ngay không?",
                                           "Thẻ hết hạn", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ExtendMembership();
                }
                return;
            }

            // ENHANCED: Cảnh báo nếu thẻ sắp hết hạn
            string warningMessage = "";
            if (SelectedMemberInfo.DaysRemaining <= 7 && SelectedMemberInfo.DaysRemaining > 0)
            {
                warningMessage = $"\n⚠️ Lưu ý: Thẻ tập sắp hết hạn sau {SelectedMemberInfo.DaysRemaining} ngày!";
            }

            var confirmResult = MessageBox.Show($"Check-in cho thành viên: {SelectedMemberInfo.FullName}?\n" +
                                              $"Gói tập: {SelectedMemberInfo.PackageName}\n" +
                                              $"Thẻ còn hiệu lực: {SelectedMemberInfo.DaysRemaining} ngày\n" +
                                              $"Thời gian: {DateTime.Now:HH:mm dd/MM/yyyy}{warningMessage}",
                                              "Xác nhận check-in", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    // ENHANCED: Kiểm tra check-in trùng lặp trong ngày
                    string checkDuplicateSql = @"SELECT COUNT(*) FROM CheckInLog 
                                                WHERE MemberId = :memberId 
                                                AND TRUNC(CheckInTime) = TRUNC(SYSDATE)
                                                AND CheckOutTime IS NULL";

                    int todayCheckIns = 0;
                    using (var checkCmd = new OracleCommand(checkDuplicateSql, connection))
                    {
                        checkCmd.Parameters.Add(":memberId", SelectedMemberInfo.Id);
                        var result = checkCmd.ExecuteScalar();
                        todayCheckIns = result != null ? Convert.ToInt32(result) : 0;
                    }

                    if (todayCheckIns > 0)
                    {
                        var duplicateResult = MessageBox.Show($"{SelectedMemberInfo.FullName} đã check-in hôm nay!\n" +
                                                            "Bạn có muốn check-in lại không?",
                                                            "Đã check-in hôm nay", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (duplicateResult == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    // ENHANCED: Thực hiện check-in với thông tin chi tiết hơn
                    string sql = @"INSERT INTO CheckInLog (MemberId, CheckInTime, Notes) 
                                  VALUES (:memberId, SYSDATE, :notes)";

                    using var cmd = new OracleCommand(sql, connection);
                    cmd.Parameters.Add(":memberId", SelectedMemberInfo.Id);
                    cmd.Parameters.Add(":notes", $"Check-in từ Members Info - {SelectedMemberInfo.PackageName} - Còn {SelectedMemberInfo.DaysRemaining} ngày");

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // ENHANCED: Thông báo thành công với thông tin chi tiết
                        string successMessage = $"Check-in thành công!\n\n" +
                                              $"Thành viên: {SelectedMemberInfo.FullName}\n" +
                                              $"Thời gian: {DateTime.Now:HH:mm dd/MM/yyyy}\n" +
                                              $"Gói tập: {SelectedMemberInfo.PackageName}\n" +
                                              $"Thẻ còn hiệu lực: {SelectedMemberInfo.DaysRemaining} ngày";

                        if (SelectedMemberInfo.DaysRemaining <= 7)
                        {
                            successMessage += $"\n\n Nhắc nhở: Thẻ tập sắp hết hạn!";
                        }

                        MessageBox.Show(successMessage, "Check-in thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                        // ENHANCED: Tự động refresh data để cập nhật thống kê
                        LoadData();
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