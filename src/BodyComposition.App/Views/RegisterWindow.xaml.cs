using System;
using System.Windows;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views
{
    public partial class RegisterWindow : Window
    {
        public int? CreatedAccountNo { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
            FlowDirection = App.Localization.FlowDirection;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(HeightBox.Text) ||
                BirthdayPicker.SelectedDate == null)
            {
                ShowError((string)FindResource("Str_RequiredFieldsMissing"));
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowError((string)FindResource("Str_PasswordMismatch"));
                return;
            }

            if (!double.TryParse(HeightBox.Text, out var height) || height <= 0)
            {
                ShowError((string)FindResource("Str_RequiredFieldsMissing"));
                return;
            }

            var profile = new UserProfile
            {
                Name = NameBox.Text.Trim(),
                Sex = SexCombo.SelectedIndex == 1 ? Gender.Female : Gender.Male,
                DailyPhysicalLabor = (ActivityLevel)ActivityCombo.SelectedIndex,
                Race = (Race)RaceCombo.SelectedIndex,
                Birthday = BirthdayPicker.SelectedDate.Value,
                HeightCm = height,
                TelMobile = TelBox.Text,
                Address = AddressBox.Text
            };

            CreatedAccountNo = App.Db.CreateUser(profile, PasswordBox.Password);
            DialogResult = true;
            Close();
        }

        private void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message)) { ErrorText.Visibility = Visibility.Collapsed; return; }
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
