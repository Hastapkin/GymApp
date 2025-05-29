using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using GymApp.ViewModels.Members_Info;

namespace GymApp.Views.Members_Info
{
    public partial class Members_InfoCreateView : Window
    {
        private Members_InfoCreateViewModel _viewModel;

        public Members_InfoCreateView()
        {
            InitializeComponent();
            _viewModel = new Members_InfoCreateViewModel();
            _viewModel.RequestClose += OnRequestClose;
            DataContext = _viewModel;
        }

        // Event handler được khai báo trong XAML
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FullNameTextBox?.Focus();
        }

        // Event handler được khai báo trong XAML  
        private void FullNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PhoneTextBox?.Focus();
            }
        }

        // Event handler được khai báo trong XAML
        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Event handler được khai báo trong XAML
        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (!IsValidEmail(textBox.Text))
                {
                    MessageBox.Show("Email không đúng định dạng!", "Cảnh báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    textBox.Focus();
                }
            }
        }

        // Event handler được khai báo trong XAML
        private void PackageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedPackage != null)
            {
                System.Diagnostics.Debug.WriteLine($"Selected package: {_viewModel.SelectedPackage.PackageName}");
            }
        }

        // Event handler được khai báo trong XAML
        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedPackage != null)
            {
                System.Diagnostics.Debug.WriteLine($"Start date: {_viewModel.MembershipStartDate:dd/MM/yyyy}");
            }
        }

        // Event handler được khai báo trong XAML
        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Event handler được khai báo trong XAML
        private void SaveButton_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(46, 204, 113));
            }
        }

        private void OnRequestClose(bool success)
        {
            this.DialogResult = success;
            this.Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.RequestClose -= OnRequestClose;
            }
            base.OnClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_viewModel?.CancelCommand?.CanExecute(null) == true)
                {
                    _viewModel.CancelCommand.Execute(null);
                }
            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_viewModel?.SaveCommand?.CanExecute(null) == true)
                {
                    _viewModel.SaveCommand.Execute(null);
                }
            }
            base.OnKeyDown(e);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}