using GymApp.Commands;
using GymApp.Models;
using GymApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GymApp.ViewModels
{
    public class StaffViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Staff> _staffList;
        private Staff _selectedStaff;

        public ObservableCollection<Staff> StaffList
        {
            get => _staffList;
            set
            {
                _staffList = value;
                OnPropertyChanged(nameof(StaffList));
            }
        }

        public Staff SelectedStaff
        {
            get => _selectedStaff;
            set
            {
                _selectedStaff = value;
                OnPropertyChanged(nameof(SelectedStaff));
            }
        }

        public ICommand AddStaffCommand { get; }
        public ICommand EditStaffCommand { get; }
        public ICommand DeleteStaffCommand { get; }
        public ICommand RefreshCommand { get; }

        public StaffViewModel()
        {
            _databaseService = new DatabaseService();
            StaffList = new ObservableCollection<Staff>();

            AddStaffCommand = new RelayCommand(AddStaff);
            EditStaffCommand = new RelayCommand(EditStaff, () => SelectedStaff != null);
            DeleteStaffCommand = new RelayCommand(DeleteStaff, () => SelectedStaff != null);
            RefreshCommand = new RelayCommand(LoadStaff);

            LoadStaff();
        }

        private void LoadStaff()
        {
            try
            {
                var staff = _databaseService.GetAllStaff();
                StaffList.Clear();
                foreach (var s in staff)
                {
                    StaffList.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách nhân viên: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddStaff()
        {
            var newStaff = new Staff
            {
                FullName = "Nhân viên mới",
                Phone = "",
                Email = "",
                Role = "Thu ngân",
                StartDate = DateTime.Now,
                Salary = 0,
                IsActive = true
            };

            try
            {
                _databaseService.AddStaff(newStaff);
                LoadStaff();
                MessageBox.Show("Thêm nhân viên thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm nhân viên: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditStaff()
        {
            if (SelectedStaff == null) return;

            try
            {
                _databaseService.UpdateStaff(SelectedStaff);
                MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật nhân viên: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteStaff()
        {
            if (SelectedStaff == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa nhân viên {SelectedStaff.FullName}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _databaseService.DeleteStaff(SelectedStaff.Id);
                    LoadStaff();
                    MessageBox.Show("Xóa nhân viên thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa nhân viên: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}