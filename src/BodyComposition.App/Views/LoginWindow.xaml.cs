using System.Windows;
using System.Windows.Input;

namespace BodyComposition.App.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            FlowDirection = App.Localization.FlowDirection;
            App.Localization.LanguageChanged += () => FlowDirection = App.Localization.FlowDirection;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void LanguageToggle_Click(object sender, RoutedEventArgs e) =>
            App.Localization.ToggleLanguage();

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            var userId = UserIdCombo.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                ShowError((string)FindResource("Str_LoginError"));
                return;
            }

            var user = App.Db.TryLogin(userId, password);
            if (user == null)
            {
                ShowError((string)FindResource("Str_LoginError"));
                return;
            }

            App.CurrentUser = user;
            new DashboardWindow(user).Show();
            Close();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow();
            reg.ShowDialog();
        }

        private void VisitorMode_Click(object sender, RoutedEventArgs e)
        {
            var visitor = new VisitorModeWindow();
            visitor.Show();
        }

        private void AdminMode_Click(object sender, RoutedEventArgs e)
        {
            var userId = UserIdCombo.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            {
                ShowError((string)FindResource("Str_LoginError"));
                return;
            }

            var user = App.Db.TryLogin(userId, password);
            if (user == null || !user.IsAdmin)
            {
                ShowError((string)FindResource("Str_LoginError"));
                return;
            }

            App.CurrentUser = user;
            new DashboardWindow(user).Show();
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
