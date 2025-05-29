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
                OnPropertyChanged(nameof(ExtensionDays)); // ✅ Trigger ExtensionDays update
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

        // MISSING PROPERTY: ExtensionDays được sử dụng trong XAML
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
                // IMPROVED: Tính giá dựa trên số ngày gia hạn
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
                    $"Giá gia hạn: {ExtensionPrice:N0} VNĐ",
                    "Xác nhận gia hạn", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using var connection = _dbContext.GetConnection();
                    connection.Open();

                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        // FIX: Tìm thẻ tập theo MemberId và EndDate
                        string getMembershipSql = @"SELECT Id, Price FROM MembershipCards 
                                           WHERE MemberId = :memberId 
                                           AND EndDate = :currentEndDate 
                                           AND Status = :status
                                           ORDER BY Id DESC 
                                           FETCH FIRST 1 ROWS ONLY";

                        int membershipCardId = 0;
                        decimal currentPrice = 0;
                        using (var getCmd = new OracleCommand(getMembershipSql, connection))
                        {
                            getCmd.Transaction = transaction;

                            // FIX: Sử dụng OracleDbType để tránh character set mismatch
                            var memberIdParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":memberId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
                            memberIdParam.Value = MemberInfo.Id;
                            getCmd.Parameters.Add(memberIdParam);

                            var endDateParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":currentEndDate", Oracle.ManagedDataAccess.Client.OracleDbType.Date);
                            endDateParam.Value = MemberInfo.EndDate;
                            getCmd.Parameters.Add(endDateParam);

                            var statusParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":status", Oracle.ManagedDataAccess.Client.OracleDbType.NVarchar2);
                            statusParam.Value = "Hoạt động";
                            getCmd.Parameters.Add(statusParam);

                            using var reader = getCmd.ExecuteReader();
                            if (reader.Read())
                            {
                                membershipCardId = Convert.ToInt32(reader["Id"]);
                                currentPrice = Convert.ToDecimal(reader["Price"]);
                            }
                        }

                        if (membershipCardId > 0)
                        {
                            // FIX: Update thẻ tập với explicit parameter types
                            string updateSql = @"UPDATE MembershipCards 
                                       SET EndDate = :newEndDate, 
                                           Price = :newPrice,
                                           Notes = NVL(Notes, '') || :extensionNote
                                       WHERE Id = :membershipId";

                            using var updateCmd = new OracleCommand(updateSql, connection);
                            updateCmd.Transaction = transaction;

                            // FIX: Explicit parameter binding
                            var newEndDateParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":newEndDate", Oracle.ManagedDataAccess.Client.OracleDbType.Date);
                            newEndDateParam.Value = NewEndDate;
                            updateCmd.Parameters.Add(newEndDateParam);

                            var newPriceParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":newPrice", Oracle.ManagedDataAccess.Client.OracleDbType.Decimal);
                            newPriceParam.Value = currentPrice + ExtensionPrice;
                            updateCmd.Parameters.Add(newPriceParam);

                            var extensionNoteParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":extensionNote", Oracle.ManagedDataAccess.Client.OracleDbType.NVarchar2);
                            extensionNoteParam.Value = $" | Gia hạn {ExtensionDays} ngày ({DateTime.Now:dd/MM/yyyy}) - Phí: {ExtensionPrice:N0} VNĐ";
                            updateCmd.Parameters.Add(extensionNoteParam);

                            var membershipIdParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":membershipId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
                            membershipIdParam.Value = membershipCardId;
                            updateCmd.Parameters.Add(membershipIdParam);

                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // FIX: Log gia hạn với explicit parameters
                                string logSql = @"INSERT INTO CheckInLog (MemberId, CheckInTime, Notes) 
                                        VALUES (:memberId, SYSDATE, :notes)";

                                using var logCmd = new OracleCommand(logSql, connection);
                                logCmd.Transaction = transaction;

                                var logMemberIdParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":memberId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
                                logMemberIdParam.Value = MemberInfo.Id;
                                logCmd.Parameters.Add(logMemberIdParam);

                                var logNotesParam = new Oracle.ManagedDataAccess.Client.OracleParameter(":notes", Oracle.ManagedDataAccess.Client.OracleDbType.NVarchar2);
                                logNotesParam.Value = $"Gia hạn thẻ tập {ExtensionDays} ngày - Phí: {ExtensionPrice:N0} VNĐ";
                                logCmd.Parameters.Add(logNotesParam);

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