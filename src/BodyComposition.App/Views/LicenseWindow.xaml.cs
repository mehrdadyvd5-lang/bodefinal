using System.Windows;
using System.Windows.Input;

namespace BodyComposition.App.Views
{
    public partial class LicenseWindow : Window
    {
        public string LicenseCode { get; set; }

        public LicenseWindow()
        {
            InitializeComponent();
            DataContext = this;
            FlowDirection = App.Localization.FlowDirection;
            App.Localization.LanguageChanged += () => FlowDirection = App.Localization.FlowDirection;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LanguageToggle_Click(object sender, RoutedEventArgs e)
        {
            App.Localization.ToggleLanguage();
        }

        private async void Activate_Click(object sender, RoutedEventArgs e)
        {
            ShowError(null);

            if (string.IsNullOrWhiteSpace(LicenseCodeBox.Text))
            {
                ShowError((string)FindResource("Str_LicenseInvalid"));
                return;
            }

            Spinner.Visibility = Visibility.Visible;
            IsEnabled = false;

            var result = await App.License.ActivateAsync(LicenseCodeBox.Text);

            Spinner.Visibility = Visibility.Collapsed;
            IsEnabled = true;

            if (result.IsValid)
            {
                DialogResult = true;
                Close();
                return;
            }

            if (result.NetworkError)
                ShowError((string)FindResource("Str_LicenseNoInternet"));
            else
                ShowError(string.IsNullOrEmpty(result.Message)
                    ? (string)FindResource("Str_LicenseInvalid")
                    : result.Message);
        }

        private void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                StatusText.Visibility = Visibility.Collapsed;
                return;
            }
            StatusText.Text = message;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}
