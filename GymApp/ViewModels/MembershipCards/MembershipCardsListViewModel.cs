using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GymApp.Data;
using GymApp.Helpers;
using GymApp.Models;

namespace GymApp.ViewModels.Membership
{
    public class MembershipCardsListViewModel : INotifyPropertyChanged
    {
        private readonly DbContext _dbContext;
        private ObservableCollection<MembershipCards> _membershipCards;
        private MembershipCards? _selectedMembershipCard;
        private object? _currentView;

        public MembershipCardsListViewModel()
        {
            _dbContext = new DbContext();
            _membershipCards = new ObservableCollection<MembershipCards>();

            CreateMembershipCardCommand = new RelayCommand(CreateMembershipCard);
            EditMembershipCardCommand = new RelayCommand(EditMembershipCard, CanEditMembershipCard);
            DeleteMembershipCardCommand = new RelayCommand(DeleteMembershipCard, CanDeleteMembershipCard);

            LoadMembershipCards();
        }

        public ObservableCollection<MembershipCards> MembershipCards
        {
            get => _membershipCards;
            set { _membershipCards = value; OnPropertyChanged(nameof(MembershipCards)); }
        }

        public MembershipCards? SelectedMembershipCard
        {
            get => _selectedMembershipCard;
            set { _selectedMembershipCard = value; OnPropertyChanged(nameof(SelectedMembershipCard)); }
        }

        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand CreateMembershipCardCommand { get; }
        public ICommand EditMembershipCardCommand { get; }
        public ICommand DeleteMembershipCardCommand { get; }

        private async void LoadMembershipCards()
        {
            try
            {
                var cards = await _dbContext.GetMembershipCardsAsync();
                MembershipCards.Clear();
                foreach (var card in cards)
                    MembershipCards.Add(card);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading membership cards: {ex.Message}");
            }
        }

        private void CreateMembershipCard(object? parameter)
        {
            var createViewModel = new MembershipCardsCreateViewModel();
            createViewModel.MembershipCardCreated += () => { LoadMembershipCards(); CurrentView = null; };
            createViewModel.CancelRequested += () => CurrentView = null;
            CurrentView = createViewModel;
        }

        private bool CanEditMembershipCard(object? parameter) => SelectedMembershipCard != null;

        private void EditMembershipCard(object? parameter)
        {
            if (SelectedMembershipCard != null)
            {
                var editViewModel = new MembershipCardsEditViewModel(SelectedMembershipCard);
                editViewModel.MembershipCardUpdated += () => { LoadMembershipCards(); CurrentView = null; };
                editViewModel.CancelRequested += () => CurrentView = null;
                CurrentView = editViewModel;
            }
        }

        private bool CanDeleteMembershipCard(object? parameter) => SelectedMembershipCard != null;

        private async void DeleteMembershipCard(object? parameter)
        {
            if (SelectedMembershipCard != null)
            {
                try
                {
                    await _dbContext.DeleteMembershipCardAsync(SelectedMembershipCard.Id);
                    LoadMembershipCards();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting membership card: {ex.Message}");
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}