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
                    // Gia hạn từ ngày kết thúc hiện tại
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

        public int ExtensionDays => Math.Max(0, (NewEndDate - MemberInfo.EndDate).Days);

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
                // ✅ IMPROVED: Tính giá dựa trên số ngày gia hạn
                var daysToExtend = ExtensionDays;
                if (daysToExtend > 0 && MemberInfo.Price > 0)
                {
                    // Giả sử gói hiện tại có mức giá theo ngày
                    var currentPackageDays = (MemberInfo.EndDate - MemberInfo.StartDate).Days;
                    if (currentPackageDays > 0)
                    {
                        var dailyRate = MemberInfo.Price / currentPackageDays;
                        ExtensionPrice = dailyRate * daysToExtend;
                    }
                    else
                    {
                        // Fallback: Sử dụng mức giá cố định 20,000 VNĐ/ngày
                        ExtensionPrice = 20000 * daysToExtend;
                    }
                }
                else
                {
                    ExtensionPrice = 0;
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
                    $"Giá gia hạn: {ExtensionPrice:N0} VNĐ\n" +
                    $"Tổng giá mới: {(MemberInfo.Price + ExtensionPrice):N0} VNĐ",
                    "Xác nhận gia hạn", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    // ✅ IMPROVED: Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        // 1. Tìm thẻ tập hiện tại của thành viên
                        string getMembershipSql = @"SELECT Id, Price FROM MembershipCards 
                                                   WHERE MemberId = :memberId 
                                                   AND EndDate = :currentEndDate 
                                                   AND Status = 'Hoạt động'
                                                   ORDER BY Id DESC 
                                                   FETCH FIRST 1 ROWS ONLY";

                        int membershipCardId = 0;
                        decimal currentPrice = 0;
                        using (var getCmd = new OracleCommand(getMembershipSql, connection))
                        {
                            getCmd.Transaction = transaction;
                            getCmd.Parameters.Add(":memberId", MemberInfo.Id);
                            getCmd.Parameters.Add(":currentEndDate", MemberInfo.EndDate);

                            using var reader = getCmd.ExecuteReader();
                            if (reader.Read())
                            {
                                membershipCardId = Convert.ToInt32(reader["Id"]);
                                currentPrice = Convert.ToDecimal(reader["Price"]);
                            }
                        }

                        if (membershipCardId > 0)
                        {
                            // 2. Cập nhật thẻ tập hiện tại
                            string updateSql = @"UPDATE MembershipCards 
                                               SET EndDate = :newEndDate, 
                                                   Price = :newPrice,
                                                   Notes = NVL(Notes, '') || ' | Gia hạn ' || :extensionDays || ' ngày (' || TO_CHAR(SYSDATE, 'DD/MM/YYYY') || ') - Phí: ' || :extensionPrice || ' VNĐ'
                                               WHERE Id = :membershipId";

                            using var updateCmd = new OracleCommand(updateSql, connection);
                            updateCmd.Transaction = transaction;
                            updateCmd.Parameters.Add(":newEndDate", NewEndDate);
                            updateCmd.Parameters.Add(":newPrice", currentPrice + ExtensionPrice);
                            updateCmd.Parameters.Add(":extensionDays", ExtensionDays);
                            updateCmd.Parameters.Add(":extensionPrice", ExtensionPrice);
                            updateCmd.Parameters.Add(":membershipId", membershipCardId);

                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // 3. Ghi log gia hạn (tùy chọn)
                                string logSql = @"INSERT INTO CheckInLog (MemberId, CheckInTime, Notes) 
                                                VALUES (:memberId, SYSDATE, :notes)";

                                using var logCmd = new OracleCommand(logSql, connection);
                                logCmd.Transaction = transaction;
                                logCmd.Parameters.Add(":memberId", MemberInfo.Id);
                                logCmd.Parameters.Add(":notes", $"Gia hạn thẻ tập {ExtensionDays} ngày - Phí: {ExtensionPrice:N0} VNĐ");
                                logCmd.ExecuteNonQuery();

                                transaction.Commit();
                                MessageBox.Show("Gia hạn thẻ tập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                                RequestClose?.Invoke(true);
                            }
                            else
                            {
                                transaction.Rollback();
                                MessageBox.Show("Không thể cập nhật thẻ tập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            MessageBox.Show("Không tìm thấy thẻ tập hoạt động để gia hạn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
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