using System;
using System.ComponentModel;

namespace GymApp.Models;

public class Members_Info : INotifyPropertyChanged
{
    private int _id;
    private string? _fullName;
    private string? _phone;
    private string? _email;
    private string? _gender;
    private DateTime _joinDate;
    private DateTime _startDate;
    private DateTime _endDate;
    private string? _packageName;
    private decimal _price;
    private string? _status;
    private string? _membershipStatus;
    private int _daysRemaining;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    public string? FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(nameof(FullName)); }
    }

    public string? Phone
    {
        get => _phone;
        set { _phone = value; OnPropertyChanged(nameof(Phone)); }
    }

    public string? Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public string? Gender
    {
        get => _gender;
        set { _gender = value; OnPropertyChanged(nameof(Gender)); }
    }

    public DateTime JoinDate
    {
        get => _joinDate;
        set { _joinDate = value; OnPropertyChanged(nameof(JoinDate)); }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
    }

    public DateTime EndDate
    {
        get => _endDate;
        set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
    }

    public string? PackageName
    {
        get => _packageName;
        set { _packageName = value; OnPropertyChanged(nameof(PackageName)); }
    }

    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(nameof(Price)); }
    }

    public string? Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(nameof(Status)); }
    }

    public string? MembershipStatus
    {
        get => _membershipStatus;
        set { _membershipStatus = value; OnPropertyChanged(nameof(MembershipStatus)); }
    }

    public int DaysRemaining
    {
        get => _daysRemaining;
        set { _daysRemaining = value; OnPropertyChanged(nameof(DaysRemaining)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}