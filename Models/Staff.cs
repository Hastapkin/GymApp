using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models;

public class Staff : INotifyPropertyChanged
{
    private int _id;
    private string? _fullName;
    private string? _phone;
    private string? _email;
    private string? _role;
    private DateTime _startDate;
    private decimal _salary;
    private string? _address;
    private bool _isActive;
    private string? _notes;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    [Required(ErrorMessage = "Họ tên không được để trống")]
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

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    [Required(ErrorMessage = "Chức vụ không được để trống")]
    public string? Role
    {
        get => _role;
        set { _role = value; OnPropertyChanged(nameof(Role)); }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
    }

    [Range(0, double.MaxValue, ErrorMessage = "Lương phải lớn hơn hoặc bằng 0")]
    public decimal Salary
    {
        get => _salary;
        set { _salary = value; OnPropertyChanged(nameof(Salary)); }
    }

    public string? Address
    {
        get => _address;
        set { _address = value; OnPropertyChanged(nameof(Address)); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
    }

    public string? Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(nameof(Notes)); }
    }

    public DateTime CreatedDate { get; set; }

    public Staff()
    {
        StartDate = DateTime.Now;
        IsActive = true;
        CreatedDate = DateTime.Now;
        Salary = 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}