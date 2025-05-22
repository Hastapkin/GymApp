using GymApp.Commands;
using GymApp.Models;
using GymApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GymApp.ViewModels
{
    public class MembersViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Member> _members;
        private Member _selectedMember;
        private string _searchText;

        public ObservableCollection<Member> Members
        {
            get => _members;
            set
            {
                _members = value;
                OnPropertyChanged(nameof(Members));
            }
        }

        public Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                _selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                SearchMembers();
            }
        }

        public ICommand AddMemberCommand { get; }
        public ICommand EditMemberCommand { get; }
        public ICommand DeleteMemberCommand { get; }
        public ICommand RefreshCommand { get; }

        public MembersViewModel()
        {
            _databaseService = new DatabaseService();
            Members = new ObservableCollection<Member>();

            AddMemberCommand = new RelayCommand(AddMember);
            EditMemberCommand = new RelayCommand(EditMember, () => SelectedMember != null);
            DeleteMemberCommand = new RelayCommand(DeleteMember, () => SelectedMember != null);
            RefreshCommand = new RelayCommand(LoadMembers);

            LoadMembers();
        }

        private void LoadMembers()
        {
            try
            {
                var members = _databaseService.GetAllMembers();
                Members.Clear();
                foreach (var member in members)
                {
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách thành viên: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMember()
        {
            var newMember = new Member
            {
                FullName = "Thành viên mới",
                Phone = "",
                Email = "",
                Gender = "Nam",
                JoinDate = DateTime.Now,
                IsActive = true
            };

            try
            {
                _databaseService.AddMember(newMember);
                LoadMembers();
                MessageBox.Show("Thêm thành viên thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm thành viên: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditMember()
        {
            if (SelectedMember == null) return;

            try
            {
                _databaseService.UpdateMember(SelectedMember);
                MessageBox.Show("Cập nhật thành viên thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi cập nhật thành viên: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteMember()
        {
            if (SelectedMember == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa thành viên {SelectedMember.FullName}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _databaseService.DeleteMember(SelectedMember.Id);
                    LoadMembers();
                    MessageBox.Show("Xóa thành viên thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa thành viên: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchMembers()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                LoadMembers();
                return;
            }

            try
            {
                var allMembers = _databaseService.GetAllMembers();
                var filteredMembers = allMembers
                    .Where(m => m.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                               (m.Phone != null && m.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                Members.Clear();
                foreach (var member in filteredMembers)
                {
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}