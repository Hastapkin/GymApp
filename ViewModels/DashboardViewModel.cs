using System.ComponentModel;
using System.Windows.Input;
using GymApp.Helpers;
using GymApp.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows;
using System;

namespace GymApp.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly DbContext _dbContext;
    private int _totalMembers;
    private int _activeMembers;
    private int _totalStaff;
    private int _totalPackages;
    private int _activeMemberships;
    private decimal _monthlyRevenue;

    public int TotalMembers
    {
        get => _totalMembers;
        set { _totalMembers = value; OnPropertyChanged(nameof(TotalMembers)); }
    }

    public int ActiveMembers
    {
        get => _activeMembers;
        set { _activeMembers = value; OnPropertyChanged(nameof(ActiveMembers)); }
    }

    public int TotalStaff
    {
        get => _totalStaff;
        set { _totalStaff = value; OnPropertyChanged(nameof(TotalStaff)); }
    }

    public int TotalPackages
    {
        get => _totalPackages;
        set { _totalPackages = value; OnPropertyChanged(nameof(TotalPackages)); }
    }

    public int ActiveMemberships
    {
        get => _activeMemberships;
        set { _activeMemberships = value; OnPropertyChanged(nameof(ActiveMemberships)); }
    }

    public decimal MonthlyRevenue
    {
        get => _monthlyRevenue;
        set { _monthlyRevenue = value; OnPropertyChanged(nameof(MonthlyRevenue)); }
    }

    public ICommand RefreshCommand { get; }

    public DashboardViewModel()
    {
        _dbContext = new DbContext();
        RefreshCommand = new RelayCommand(LoadDashboardData);
        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        try
        {
            using var connection = _dbContext.GetConnection();
            connection.Open();

            // Total Members
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM Members", connection))
            {
                var result = cmd.ExecuteScalar();
                TotalMembers = result != null ? Convert.ToInt32(result) : 0;
            }

            // Active Members
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM Members WHERE IsActive = 1", connection))
            {
                var result = cmd.ExecuteScalar();
                ActiveMembers = result != null ? Convert.ToInt32(result) : 0;
            }

            // Total Staff
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM Staff", connection))
            {
                var result = cmd.ExecuteScalar();
                TotalStaff = result != null ? Convert.ToInt32(result) : 0;
            }

            // Total Packages
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM Packages WHERE IsActive = 1", connection))
            {
                var result = cmd.ExecuteScalar();
                TotalPackages = result != null ? Convert.ToInt32(result) : 0;
            }

            // Active Memberships
            using (var cmd = new OracleCommand("SELECT COUNT(*) FROM MembershipCards WHERE Status = 'Hoạt động' AND EndDate >= SYSDATE", connection))
            {
                var result = cmd.ExecuteScalar();
                ActiveMemberships = result != null ? Convert.ToInt32(result) : 0;
            }

            // Monthly Revenue
            using (var cmd = new OracleCommand("SELECT NVL(SUM(Price), 0) FROM MembershipCards WHERE EXTRACT(MONTH FROM CreatedDate) = EXTRACT(MONTH FROM SYSDATE) AND EXTRACT(YEAR FROM CreatedDate) = EXTRACT(YEAR FROM SYSDATE)", connection))
            {
                var result = cmd.ExecuteScalar();
                MonthlyRevenue = result != null ? Convert.ToDecimal(result) : 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu dashboard: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}