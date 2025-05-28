using System;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Models;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GymApp.ViewModels.MembershipCards
{
    public class MembershipCardsCreateViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private Models.MembershipCards _membershipCard;
        private ObservableCollection<Models.Member> _members;
        private ObservableCollection<Models.Packages> _packages;
        private Models.Member _selectedMember;
        private Models.Packages _selectedPackage;

        public Models.MembershipCards MembershipCard
        {
            get => _membershipCard;
            set { _membershipCard = value; OnPropertyChanged(nameof(MembershipCard)); }
        }

        public ObservableCollection<Models.Member> Members
        {
            get => _members;
            set { _members = value; OnPropertyChanged(nameof(Members)); }
        }

        public ObservableCollection<Models.Packages> Packages
        {
            get => _packages;
            set { _packages = value; OnPropertyChanged(nameof(Packages)); }
        }

        public Models.Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
                if (value != null)
                {
                    MembershipCard.MemberId = value.Id;
                }
            }
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
                    MembershipCard.PackageId = value.Id;
                    MembershipCard.Price = value.Price;
                    // Auto calculate end date based on package duration
                    MembershipCard.EndDate = MembershipCard.StartDate.AddDays(value.DurationDays);
                }
            }
        }

        public List<string> PaymentMethods { get; } = new List<string> { "Tiền mặt", "Chuyển khoản", "Thẻ tín dụng" };
        public List<string> StatusOptions { get; } = new List<string> { "Hoạt động", "Hết hạn", "Tạm ngưng" };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<bool> RequestClose;

        public MembershipCardsCreateViewModel()
        {
            _dbContext = new DbContext();
            MembershipCard = new Models.MembershipCards();
            Members = new ObservableCollection<Models.Member>();
            Packages = new ObservableCollection<Models.Packages>();

            SaveCommand = new RelayCommand(SaveMembershipCard);
            CancelCommand = new RelayCommand(Cancel);

            LoadMembers();
            LoadPackages();
        }

        private void LoadMembers()
        {
            try
            {
                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = "SELECT Id, FullName, Phone FROM Members WHERE IsActive = 1 ORDER BY FullName";
                using var cmd = new OracleCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var member = new Models.Member
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"]?.ToString(),
                        Phone = reader["Phone"]?.ToString()
                    };
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách thành viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void SaveMembershipCard()
        {
            try
            {
                // Validate required fields
                if (MembershipCard.MemberId <= 0)
                {
                    MessageBox.Show("Vui lòng chọn thành viên!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MembershipCard.PackageId <= 0)
                {
                    MessageBox.Show("Vui lòng chọn gói tập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MembershipCard.StartDate >= MembershipCard.EndDate)
                {
                    MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MembershipCard.Price <= 0)
                {
                    MessageBox.Show("Giá phải lớn hơn 0!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var connection = _dbContext.GetConnection();
                connection.Open();

                string sql = @"INSERT INTO MembershipCards (MemberId, PackageId, StartDate, EndDate, Price, 
                              PaymentMethod, Status, Notes, CreatedDate, CreatedBy) 
                              VALUES (:memberId, :packageId, :startDate, :endDate, :price, 
                              :paymentMethod, :status, :notes, :createdDate, :createdBy)";

                using var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.Add(":memberId", MembershipCard.MemberId);
                cmd.Parameters.Add(":packageId", MembershipCard.PackageId);
                cmd.Parameters.Add(":startDate", MembershipCard.StartDate);
                cmd.Parameters.Add(":endDate", MembershipCard.EndDate);
                cmd.Parameters.Add(":price", MembershipCard.Price);
                cmd.Parameters.Add(":paymentMethod", MembershipCard.PaymentMethod ?? "Tiền mặt");
                cmd.Parameters.Add(":status", MembershipCard.Status ?? "Hoạt động");
                cmd.Parameters.Add(":notes", MembershipCard.Notes ?? (object)DBNull.Value);
                cmd.Parameters.Add(":createdDate", DateTime.Now);
                cmd.Parameters.Add(":createdBy", MembershipCard.CreatedBy ?? "Admin");

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Tạo thẻ tập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo thẻ tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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