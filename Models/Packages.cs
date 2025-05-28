using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models;

public class Packages : INotifyPropertyChanged
{
    private int _id;
    private string? _packageName;
    private string? _description;
    private int _durationDays;
    private decimal _price;
    private bool _isActive;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    [Required(ErrorMessage = "Tên gói không được để trống")]
    public string? PackageName
    {
        get => _packageName;
        set { _packageName = value; OnPropertyChanged(nameof(PackageName)); }
    }

    public string? Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(nameof(Description)); }
    }

    [Required(ErrorMessage = "Số ngày không được để trống")]
    [Range(1, int.MaxValue, ErrorMessage = "Số ngày phải lớn hơn 0")]
    public int DurationDays
    {
        get => _durationDays;
        set { _durationDays = value; OnPropertyChanged(nameof(DurationDays)); }
    }

    [Required(ErrorMessage = "Giá không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(nameof(Price)); }
    }

    public bool IsActive
    {
        get => _isActive;
        set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
    }

    public DateTime CreatedDate { get; set; }

    public Packages()
    {
        IsActive = true;
        CreatedDate = DateTime.Now;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}