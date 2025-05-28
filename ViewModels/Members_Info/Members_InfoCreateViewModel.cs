using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace GymApp.ViewModels.Members_Info
{
    public class Members_InfoCreateViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private Models.Member _newMember;
        private Models.Packages _selectedPackage;
        private ObservableCollection<Models.Packages> _packages;
        private DateTime _membershipStartDate;
        private DateTime _membershipEndDate;
        private decimal _membershipPrice;
        private string _paymentMethod;

        public Models.Member NewMember
        {
            get => _newMember;
            set { _newMember = value; OnPropertyChanged(nameof(NewMember)); }
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
                    MembershipEndDate = MembershipStartDate.AddDays(value.DurationDays);
                    MembershipPrice = value.Price;
                }
            }
        }

        public ObservableCollection<Models.Packages> Packages
        {
            get => _packages;
            set { _packages = value; OnPropertyChanged(nameof(Packages)); }
        }

        public DateTime MembershipStartDate
        {
            get => _membershipStartDate;
            set
            {
                _membershipStartDate = value;
                OnPropertyChanged(nameof(MembershipStartDate));
                if (SelectedPackage != null)
                {
                    MembershipEndDate = value.AddDays(SelectedPackage.DurationDays);
                }
            }
        }

        public DateTime MembershipEndDate
        {
            get => _membershipEndDate;
            set { _membershipEndDate = value; OnPropertyChanged(nameof(MembershipEndDate)); }
        }

        public decimal MembershipPrice
        {
            get => _membershipPrice;
            set { _membershipPrice = value; OnPropertyChanged(nameof(MembershipPrice)); }
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); }
        }

        public List<string> GenderOptions { get; } = new List<string> { "Nam", "Nữ", "Khác" };
        public List<string> PaymentMethods { get; } = new List<string> { "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng" };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public Members_InfoCreateViewModel()
        {
            _dbContext = new DbContext();
            NewMember = new Models.Member();
            Packages = new ObservableCollection<Models.Packages>();
            MembershipStartDate = DateTime.Now;
            MembershipEndDate = DateTime.Now.AddDays(30);
            PaymentMethod = "Tiền mặt";

            SaveCommand = new RelayCommand(SaveMemberWithMembership);
            CancelCommand = new RelayCommand(Cancel);

            LoadPackages();
        }

        private void LoadPackages()
        {
            try
            {
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = "SELECT Id, PackageName, DurationDays, Price FROM Packages WHERE IsActive = 1 ORDER BY PackageName";
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

        private void SaveMemberWithMembership()
        {
            try
            {
                // Validate member info
                if (string.IsNullOrWhiteSpace(NewMember.FullName))
                {
                    MessageBox.Show("Vui lòng nhập họ tên thành viên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate membership info
                if (SelectedPackage == null)
                {
                    MessageBox.Show("Vui lòng chọn gói tập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MembershipStartDate >= MembershipEndDate)
                {
                    MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MembershipPrice <= 0)
                {
                    MessageBox.Show("Giá thẻ tập phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var connection = _dbContext.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    // ✅ FIX: 1. Insert Member - Sử dụng IDENTITY sequence
                    string memberSql = @"INSERT INTO Members (FullName, Phone, Email, Gender, DateOfBirth, Address, 
                                        JoinDate, IsActive, Notes, CreatedDate, UpdatedDate) 
                                        VALUES (:fullName, :phone, :email, :gender, :dateOfBirth, :address, 
                                        :joinDate, :isActive, :notes, :createdDate, :updatedDate)
                                        RETURNING Id INTO :memberId";

                    int memberId;
                    using (var memberCmd = new OracleCommand(memberSql, connection))
                    {
                        memberCmd.Transaction = transaction;
                        memberCmd.Parameters.Add(":fullName", NewMember.FullName);
                        memberCmd.Parameters.Add(":phone", NewMember.Phone ?? (object)DBNull.Value);
                        memberCmd.Parameters.Add(":email", NewMember.Email ?? (object)DBNull.Value);
                        memberCmd.Parameters.Add(":gender", NewMember.Gender ?? (object)DBNull.Value);
                        memberCmd.Parameters.Add(":dateOfBirth", NewMember.DateOfBirth ?? (object)DBNull.Value);
                        memberCmd.Parameters.Add(":address", NewMember.Address ?? (object)DBNull.Value);
                        memberCmd.Parameters.Add(":joinDate", NewMember.JoinDate);
                        memberCmd.Parameters.Add(":isActive", NewMember.IsActive ? 1 : 0);
                        memberCmd.Parameters.Add(":notes", NewMember.Notes ?? (object)DBNull.Value);
                        memberCmd.Parameters.Add(":createdDate", DateTime.Now);
                        memberCmd.Parameters.Add(":updatedDate", DateTime.Now);

                        var memberIdParam = new OracleParameter(":memberId", OracleDbType.Int32)
                        {
                            Direction = System.Data.ParameterDirection.Output
                        };
                        memberCmd.Parameters.Add(memberIdParam);

                        memberCmd.ExecuteNonQuery();
                        memberId = Convert.ToInt32(memberIdParam.Value);
                    }

                    // ✅ FIX: 2. Insert MembershipCard - Sử dụng IDENTITY sequence
                    string membershipSql = @"INSERT INTO MembershipCards (MemberId, PackageId, StartDate, EndDate, Price, 
                                           PaymentMethod, Status, Notes, CreatedDate, CreatedBy) 
                                           VALUES (:memberId, :packageId, :startDate, :endDate, :price, 
                                           :paymentMethod, :status, :notes, :createdDate, :createdBy)";

                    using (var membershipCmd = new OracleCommand(membershipSql, connection))
                    {
                        membershipCmd.Transaction = transaction;
                        membershipCmd.Parameters.Add(":memberId", memberId);
                        membershipCmd.Parameters.Add(":packageId", SelectedPackage.Id);
                        membershipCmd.Parameters.Add(":startDate", MembershipStartDate);
                        membershipCmd.Parameters.Add(":endDate", MembershipEndDate);
                        membershipCmd.Parameters.Add(":price", MembershipPrice);
                        membershipCmd.Parameters.Add(":paymentMethod", PaymentMethod);
                        membershipCmd.Parameters.Add(":status", "Hoạt động");
                        membershipCmd.Parameters.Add(":notes", $"Tạo thành viên và thẻ tập cùng lúc - {SelectedPackage.PackageName}");
                        membershipCmd.Parameters.Add(":createdDate", DateTime.Now);
                        membershipCmd.Parameters.Add(":createdBy", "Admin");

                        membershipCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show($"Tạo thành viên và thẻ tập thành công!\nThành viên ID: {memberId}\nThẻ tập: {SelectedPackage.PackageName}",
                                  "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo thành viên và thẻ tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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