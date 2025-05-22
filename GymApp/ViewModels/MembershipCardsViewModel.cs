using GymApp.Commands;
using GymApp.Models;
using GymApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GymApp.ViewModels
{
    public class MembershipCardsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<MembershipCard> _membershipCards;
        private MembershipCard _selectedCard;

        public ObservableCollection<MembershipCard> MembershipCards
        {
            get => _membershipCards;
            set
            {
                _membershipCards = value;
                OnPropertyChanged(nameof(MembershipCards));
            }
        }

        public MembershipCard SelectedCard
        {
            get => _selectedCard;
            set
            {
                _selectedCard = value;
                OnPropertyChanged(nameof(SelectedCard));
            }
        }

        public ICommand ExtendMembershipCommand { get; }
        public ICommand AddMembershipCommand { get; }
        public ICommand RefreshCommand { get; }

        public MembershipCardsViewModel()
        {
            _databaseService = new DatabaseService();
            MembershipCards = new ObservableCollection<MembershipCard>();

            ExtendMembershipCommand = new RelayCommand(ExtendMembership, () => SelectedCard != null);
            AddMembershipCommand = new RelayCommand(AddMembership);
            RefreshCommand = new RelayCommand(LoadMembershipCards);

            LoadMembershipCards();
        }

        private void LoadMembershipCards()
        {
            try
            {
                var cards = _databaseService.GetAllMembershipCards();
                MembershipCards.Clear();
                foreach (var card in cards)
                {
                    MembershipCards.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách thẻ tập: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtendMembership()
        {
            if (SelectedCard == null) return;

            try
            {
                var newCard = new MembershipCard
                {
                    MemberId = SelectedCard.MemberId,
                    PackageId = SelectedCard.PackageId,
                    StartDate = SelectedCard.EndDate.AddDays(1),
                    EndDate = SelectedCard.EndDate.AddDays(31), // Extend 1 month
                    Price = SelectedCard.Price,
                    PaymentMethod = "Tiền mặt",
                    Status = "Hoạt động"
                };

                _databaseService.AddMembershipCard(newCard);
                LoadMembershipCards();
                MessageBox.Show("Gia hạn thẻ tập thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gia hạn thẻ tập: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMembership()
        {
            try
            {
                var members = _databaseService.GetAllMembers();
                var packages = _databaseService.GetAllPackages();

                if (members.Count == 0)
                {
                    MessageBox.Show("Chưa có thành viên nào. Vui lòng thêm thành viên trước!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (packages.Count == 0)
                {
                    MessageBox.Show("Chưa có gói tập nào. Vui lòng liên hệ quản trị viên!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // For demo, create membership for first member with first package
                var firstMember = members[0];
                var firstPackage = packages[0];

                var newCard = new MembershipCard
                {
                    MemberId = firstMember.Id,
                    PackageId = firstPackage.Id,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(firstPackage.DurationDays),
                    Price = firstPackage.Price,
                    PaymentMethod = "Tiền mặt",
                    Status = "Hoạt động"
                };

                _databaseService.AddMembershipCard(newCard);
                LoadMembershipCards();
                MessageBox.Show("Thêm thẻ tập thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm thẻ tập: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}