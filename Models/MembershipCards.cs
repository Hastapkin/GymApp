using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models;

public class MembershipCards : INotifyPropertyChanged
{
    private int _id;
    private int _memberId;
    private int _packageId;
    private DateTime _startDate;
    private DateTime _endDate;
    private decimal _price;
    private string? _paymentMethod;
    private string? _status;
    private string? _notes;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(nameof(Id)); }
    }

    [Required(ErrorMessage = "Thành viên không được để trống")]
    public int MemberId
    {
        get => _memberId;
        set { _memberId = value; OnPropertyChanged(nameof(MemberId)); }
    }

    [Required(ErrorMessage = "Gói tập không được để trống")]
    public int PackageId
    {
        get => _packageId;
        set { _packageId = value; OnPropertyChanged(nameof(PackageId)); }
    }

    [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
    public DateTime StartDate
    {
        get => _startDate;
        set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
    }

    [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
    public DateTime EndDate
    {
        get => _endDate;
        set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
    }

    [Required(ErrorMessage = "Giá không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(nameof(Price)); }
    }

    public string? PaymentMethod
    {
        get => _paymentMethod;
        set { _paymentMethod = value; OnPropertyChanged(nameof(PaymentMethod)); }
    }

    public string? Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(nameof(Status)); }
    }

    public string? Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(nameof(Notes)); }
    }

    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }

    // Navigation properties
    public string? MemberName { get; set; }
    public string? PackageName { get; set; }

    public MembershipCards()
    {
        StartDate = DateTime.Now;
        EndDate = DateTime.Now.AddDays(30);
        PaymentMethod = "Tiền mặt";
        Status = "Hoạt động";
        CreatedDate = DateTime.Now;
        CreatedBy = "Admin";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}