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

        // ✅ FIX: Thêm vào constructor của Members_InfoCreateViewModel

        public Members_InfoCreateViewModel()
        {
            _dbContext = new DbContext();

            // ✅ IMPORTANT: Khởi tạo tất cả objects trước
            NewMember = new Models.Member();
            Packages = new ObservableCollection<Models.Packages>();

            // ✅ Set default values
            MembershipStartDate = DateTime.Now;
            MembershipEndDate = DateTime.Now.AddDays(30);
            PaymentMethod = "Tiền mặt";

            SaveCommand = new RelayCommand(SaveMemberWithMembership);
            CancelCommand = new RelayCommand(Cancel);

            // ✅ Load packages with error handling
            try
            {
                LoadPackages();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading packages: {ex.Message}");
                // Đảm bảo Packages không null
                if (Packages == null)
                    Packages = new ObservableCollection<Models.Packages>();
            }
        }

        // ✅ Cải thiện LoadPackages method
        private void LoadPackages()
        {
            try
            {
                // ✅ Clear existing items safely
                if (Packages != null)
                    Packages.Clear();
                else
                    Packages = new ObservableCollection<Models.Packages>();

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

                // ✅ Debug info
                System.Diagnostics.Debug.WriteLine($"Loaded {Packages.Count} packages");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách gói tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                // ✅ Ensure Packages is not null even on error
                if (Packages == null)
                    Packages = new ObservableCollection<Models.Packages>();
            }
        }

        // ❌ CURRENT CODE có vấn đề với Oracle RETURNING syntax:
        string memberSql = @"INSERT INTO Members (FullName, Phone, Email, Gender, DateOfBirth, Address, 
                    JoinDate, IsActive, Notes, CreatedDate, UpdatedDate) 
                    VALUES (:fullName, :phone, :email, :gender, :dateOfBirth, :address, 
                    :joinDate, :isActive, :notes, :createdDate, :updatedDate)
                    RETURNING Id INTO :memberId"; // ❌ Syntax này không work với Oracle .NET

        // ✅ FIX: Sử dụng cách lấy ID khác
        private void SaveMemberWithMembership()
        {
            try
            {
                // Validate...

                using var connection = _dbContext.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {
                    // ✅ METHOD 1: Get next ID trước khi insert
                    string getNextIdSql = "SELECT NVL(MAX(Id), 0) + 1 FROM Members";
                    int memberId;
                    using (var getIdCmd = new OracleCommand(getNextIdSql, connection))
                    {
                        getIdCmd.Transaction = transaction;
                        var result = getIdCmd.ExecuteScalar();
                        memberId = result != null ? Convert.ToInt32(result) : 1;
                    }

                    // ✅ Insert Member với ID cụ thể
                    string memberSql = @"INSERT INTO Members (Id, FullName, Phone, Email, Gender, DateOfBirth, Address, 
                               JoinDate, IsActive, Notes, CreatedDate, UpdatedDate) 
                               VALUES (:id, :fullName, :phone, :email, :gender, :dateOfBirth, :address, 
                               :joinDate, :isActive, :notes, :createdDate, :updatedDate)";

                    using (var memberCmd = new OracleCommand(memberSql, connection))
                    {
                        memberCmd.Transaction = transaction;
                        memberCmd.Parameters.Add(":id", memberId);
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

                        memberCmd.ExecuteNonQuery();
                    }

                    // ✅ Insert MembershipCard
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