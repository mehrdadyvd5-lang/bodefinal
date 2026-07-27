using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views.Sections
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        public void Load() => Refresh(null);

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => Refresh(SearchBox.Text);

        private void Refresh(string filter)
        {
            UsersList.Items.Clear();
            var users = App.Db.GetAllUsers()
                .Where(u => string.IsNullOrWhiteSpace(filter)
                    || u.Name.ToLower().Contains(filter.ToLower())
                    || u.AccountNo.ToString() == filter)
                .ToList();

            foreach (var u in users)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                for (int i = 0; i < 7; i++) row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions[0] = new ColumnDefinition { Width = new GridLength(50) };
                row.ColumnDefinitions[1] = new ColumnDefinition { Width = new GridLength(140) };
                row.ColumnDefinitions[2] = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };

                row.Children.Add(Text(u.AccountNo.ToString(), 0));
                row.Children.Add(Text(u.Name, 1));

                var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                Grid.SetColumn(actions, 2);
                actions.Children.Add(Btn((string)FindResource("Str_ShowReport"), Color.FromRgb(0xB5,0x52,0x9E), () =>
                {
                    var records = App.Db.GetMeasurements(u.AccountNo);
                    if (records.Count > 0) new ReportWindow(u, records[0]).ShowDialog();
                    else MessageBox.Show((string)FindResource("Str_NoData"));
                }));
                actions.Children.Add(Btn((string)FindResource("Str_DeleteUser"), Color.FromRgb(0xA8,0x36,0x36), () =>
                {
                    if (Confirm()) { App.Db.DeleteUser(u.AccountNo); Refresh(SearchBox.Text); }
                }));
                actions.Children.Add(Btn((string)FindResource("Str_DeleteBodyInspectionData"), Color.FromRgb(0x7A,0x5C,0xC0), () =>
                {
                    if (Confirm()) App.Db.DeleteAllMeasurements(u.AccountNo);
                }));
                actions.Children.Add(Btn((string)FindResource("Str_DeleteQuestionnaire"), Color.FromRgb(0xB9,0x6B,0x2E), () =>
                {
                    if (Confirm()) App.Db.DeleteQuestionnaires(u.AccountNo);
                }));
                actions.Children.Add(Btn((string)FindResource("Str_ClearPassword"), Color.FromRgb(0x3D,0x8A,0x48), () =>
                {
                    if (Confirm()) App.Db.ClearPassword(u.AccountNo, "");
                }));

                row.Children.Add(actions);
                UsersList.Items.Add(row);
            }
        }

        private static bool Confirm() =>
            MessageBox.Show(Application.Current.Resources["Str_ConfirmDeleteUser"].ToString(),
                "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

        private static TextBlock Text(string s, int col)
        {
            var t = new TextBlock { Text = s, Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(t, col);
            return t;
        }

        private static Button Btn(string content, Color bg, System.Action onClick)
        {
            var b = new Button
            {
                Content = content,
                Background = new SolidColorBrush(bg),
                Foreground = Brushes.White,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(4, 0, 0, 0),
                BorderThickness = new Thickness(0),
                FontSize = 11
            };
            b.Click += (s, e) => onClick();
            return b;
        }
    }
}
