using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System.Collections.ObjectModel;

namespace GymApp.ViewModels.Members_Info
{
    public class Members_InfoEditViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private Models.Members_Info _memberInfo;
        private DateTime _newEndDate;
        private ObservableCollection<Models.Packages> _packages;
        private Models.Packages _selectedPackage;
        private decimal _extensionPrice;

        public Models.Members_Info MemberInfo
        {
            get => _memberInfo;
            set { _memberInfo = value; OnPropertyChanged(nameof(MemberInfo)); }
        }

        public DateTime NewEndDate
        {
            get => _newEndDate;
            set
            {
                _newEndDate = value;
                OnPropertyChanged(nameof(NewEndDate));
                CalculateExtensionPrice();
            }
        }

        public ObservableCollection<Models.Packages> Packages
        {
            get => _packages;
            set { _packages = value; OnPropertyChanged(nameof(Packages)); }
        }

        public Models.Packages SelectedPackage
        {
            get => _selectedPackage;
            set
            {
                _selectedPackage = value;
                OnPropertyChanged(nameof(SelectedPackage));
                if (value != null)
                {
                    NewEndDate = MemberInfo.EndDate.AddDays(value.DurationDays);
                    ExtensionPrice = value.Price;
                }
            }
        }

        public decimal ExtensionPrice
        {
            get => _extensionPrice;
            set { _extensionPrice = value; OnPropertyChanged(nameof(ExtensionPrice)); }
        }

        public int ExtensionDays => (NewEndDate - MemberInfo.EndDate).Days;

        public ICommand ExtendCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public Members_InfoEditViewModel(Models.Members_Info memberInfo)
        {
            _dbContext = new DbContext();
            MemberInfo = memberInfo;
            NewEndDate = memberInfo.EndDate.AddDays(30); // Default extend by 30 days
            Packages = new ObservableCollection<Models.Packages>();

            ExtendCommand = new RelayCommand(ExtendMembership);
            CancelCommand = new RelayCommand(Cancel);

            LoadPackages();
            CalculateExtensionPrice();
        }

        private void LoadPackages()
        {
            try
            {
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = "SELECT Id, PackageName, DurationDays, Price FROM Packages WHERE IsActive = 1 ORDER BY DurationDays";
                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var package = new Models.Packages
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        PackageName = reader["PackageName"]?.ToString(),
                        DurationDays = Convert.ToInt32(reader["DurationDays"]),
                        Price = Convert.ToDecimal(reader["Price"])
                    };
                    Packages.Add(package);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách gói tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateExtensionPrice()
        {
            if (SelectedPackage == null)
            {
                // Calculate proportional price based on days
                var daysToExtend = ExtensionDays;
                if (daysToExtend > 0 && MemberInfo.Price > 0)
                {
                    // Assume current package is 30 days for calculation
                    var dailyRate = MemberInfo.Price / 30;
                    ExtensionPrice = dailyRate * daysToExtend;
                }
            }
            OnPropertyChanged(nameof(ExtensionDays));
        }

        private void ExtendMembership()
        {
            try
            {
                if (NewEndDate <= MemberInfo.EndDate)
                {
                    MessageBox.Show("Ngày gia hạn phải sau ngày kết thúc hiện tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ExtensionPrice <= 0)
                {
                    MessageBox.Show("Giá gia hạn phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Xác nhận gia hạn thẻ tập?\n\n" +
                    $"Thành viên: {MemberInfo.FullName}\n" +
                    $"Từ: {MemberInfo.EndDate:dd/MM/yyyy}\n" +
                    $"Đến: {NewEndDate:dd/MM/yyyy}\n" +
                    $"Thêm: {ExtensionDays} ngày\n" +
                    $"Giá: {ExtensionPrice:N0} VNĐ",
                    "Xác nhận gia hạn", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    // Get the current membership card ID
                    string getMembershipSql = @"SELECT Id FROM MembershipCards 
                                               WHERE MemberId = :memberId 
                                               AND EndDate = :currentEndDate 
                                               AND Status = 'Hoạt động'
                                               ORDER BY Id DESC 
                                               FETCH FIRST 1 ROWS ONLY";

                    int membershipCardId = 0;
                    using (var getCmd = new OracleCommand(getMembershipSql, connection))
                    {
                        getCmd.Parameters.Add(":memberId", MemberInfo.Id);
                        getCmd.Parameters.Add(":currentEndDate", MemberInfo.EndDate);
                        var result2 = getCmd.ExecuteScalar();
                        if (result2 != null)
                            membershipCardId = Convert.ToInt32(result2);
                    }

                    if (membershipCardId > 0)
                    {
                        // Update existing membership card
                        string updateSql = @"UPDATE MembershipCards 
                                           SET EndDate = :newEndDate, 
                                               Price = Price + :extensionPrice,
                                               Notes = NVL(Notes, '') || ' | Gia hạn ' || :extensionDays || ' ngày (' || TO_CHAR(SYSDATE, 'DD/MM/YYYY') || ')'
                                           WHERE Id = :membershipId";

                        using var updateCmd = new OracleCommand(updateSql, connection);
                        updateCmd.Parameters.Add(":newEndDate", NewEndDate);
                        updateCmd.Parameters.Add(":extensionPrice", ExtensionPrice);
                        updateCmd.Parameters.Add(":extensionDays", ExtensionDays);
                        updateCmd.Parameters.Add(":membershipId", membershipCardId);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Gia hạn thẻ tập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            RequestClose?.Invoke(true);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thẻ tập hoạt động để gia hạn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gia hạn thẻ tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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