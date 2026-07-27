using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BodyComposition.App.Models;
using BodyComposition.App.Views.Sections;

namespace BodyComposition.App.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly UserProfile _user;
        private StartBodyInspectionView _inspectionView;
        private StatisticAnalysisView _statsView;
        private ExerciseSuggestionView _exerciseView;
        private SystemManagementView _sysMgmtView;
        private UserManagementView _userMgmtView;

        public DashboardWindow(UserProfile user)
        {
            InitializeComponent();
            _user = user;
            FlowDirection = App.Localization.FlowDirection;
            TitleUserText.Text = (_user.IsAdmin ? (string)FindResource("Str_AdminModeTitle") : $"{(string)FindResource("Str_FunctionForm")}: {_user.Name}");

            BuildSidebar();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void LanguageToggle_Click(object sender, RoutedEventArgs e) => App.Localization.ToggleLanguage();

        private void Bluetooth_Click(object sender, RoutedEventArgs e) => new BluetoothConnectWindow().ShowDialog();

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            new LoginWindow().Show();
            Close();
        }

        private void BuildSidebar()
        {
            SidebarPanel.Children.Clear();

            if (_user.IsAdmin)
            {
                SidebarPanel.Children.Add(SidebarButton((string)FindResource("Str_UserManagement"), () =>
                {
                    _userMgmtView = _userMgmtView ?? new UserManagementView();
                    _userMgmtView.Load();
                    ContentHost.Content = _userMgmtView;
                }));
                SidebarPanel.Children.Add(SidebarButton((string)FindResource("Str_MyExerciseSuggestion"), () =>
                {
                    _exerciseView = _exerciseView ?? new ExerciseSuggestionView();
                    _exerciseView.Load(_user);
                    ContentHost.Content = _exerciseView;
                }));

                _userMgmtView = new UserManagementView();
                _userMgmtView.Load();
                ContentHost.Content = _userMgmtView;
            }
            else
            {
                SidebarPanel.Children.Add(SidebarButton((string)FindResource("Str_StartBodyInspection"), () =>
                {
                    _inspectionView = _inspectionView ?? new StartBodyInspectionView();
                    _inspectionView.Load(_user);
                    ContentHost.Content = _inspectionView;
                }));
                SidebarPanel.Children.Add(SidebarButton((string)FindResource("Str_StatisticAnalysis"), () =>
                {
                    _statsView = _statsView ?? new StatisticAnalysisView();
                    _statsView.Load(_user);
                    ContentHost.Content = _statsView;
                }));
                SidebarPanel.Children.Add(SidebarButton((string)FindResource("Str_MyExerciseSuggestion"), () =>
                {
                    _exerciseView = _exerciseView ?? new ExerciseSuggestionView();
                    _exerciseView.Load(_user);
                    ContentHost.Content = _exerciseView;
                }));
                SidebarPanel.Children.Add(SidebarButton((string)FindResource("Str_SystemManagement"), () =>
                {
                    _sysMgmtView = _sysMgmtView ?? new SystemManagementView();
                    _sysMgmtView.Load(_user);
                    ContentHost.Content = _sysMgmtView;
                }));

                _inspectionView = new StartBodyInspectionView();
                _inspectionView.Load(_user);
                ContentHost.Content = _inspectionView;
            }
        }

        private Button SidebarButton(string text, System.Action onClick)
        {
            var btn = new Button
            {
                Content = text,
                Style = (Style)FindResource("PillButton"),
                Margin = new Thickness(0, 0, 0, 10),
                Height = 56,
                FontSize = 12,
                Background = new SolidColorBrush(Color.FromRgb(0x2F, 0xAE, 0x60))
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }
    }
}
