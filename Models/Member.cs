using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models;

public class Member : INotifyPropertyChanged
{
    private int _id;
    private string? _fullName;
    private string? _phone;
    private string? _email;
    private string? _gender;
    private DateTime? _dateOfBirth;
    private string? _address;
    private DateTime _joinDate;
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

    public string? Gender
    {
        get => _gender;
        set { _gender = value; OnPropertyChanged(nameof(Gender)); }
    }

    public DateTime? DateOfBirth
    {
        get => _dateOfBirth;
        set { _dateOfBirth = value; OnPropertyChanged(nameof(DateOfBirth)); }
    }

    public string? Address
    {
        get => _address;
        set { _address = value; OnPropertyChanged(nameof(Address)); }
    }

    public DateTime JoinDate
    {
        get => _joinDate;
        set { _joinDate = value; OnPropertyChanged(nameof(JoinDate)); }
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
    public DateTime UpdatedDate { get; set; }

    public Member()
    {
        JoinDate = DateTime.Now;
        IsActive = true;
        CreatedDate = DateTime.Now;
        UpdatedDate = DateTime.Now;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}