using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views.Sections
{
    public partial class SystemManagementView : UserControl
    {
        private UserProfile _user;

        public SystemManagementView()
        {
            InitializeComponent();
        }

        public void Load(UserProfile user) => _user = user;

        private void ExportInspection_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = $"{_user.Name}_inspection_records.json" };
            if (dlg.ShowDialog() != true) return;

            var records = App.Db.GetMeasurements(_user.AccountNo);
            System.IO.File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(records, Formatting.Indented));
            ShowStatus($"{(string)FindResource("Str_ExportedTo")} {dlg.FileName}");
        }

        private void ImportInspection_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON (*.json)|*.json" };
            if (dlg.ShowDialog() != true) return;

            var json = System.IO.File.ReadAllText(dlg.FileName);
            var records = JsonConvert.DeserializeObject<System.Collections.Generic.List<BodyMetrics>>(json);
            if (records != null)
            {
                foreach (var r in records)
                {
                    r.AccountNo = _user.AccountNo;
                    App.Db.SaveMeasurement(r);
                }
            }
            ShowStatus($"{(string)FindResource("Str_ImportedFrom")} {dlg.FileName}");
        }

        private void DeleteInspection_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show((string)FindResource("Str_ConfirmDeleteUser"),
                    (string)FindResource("Str_DeleteInspectionRecord"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                App.Db.DeleteAllMeasurements(_user.AccountNo);
                ShowStatus((string)FindResource("Str_DeleteInspectionRecord"));
            }
        }

        private void DeleteQuestionnaire_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show((string)FindResource("Str_ConfirmDeleteUser"),
                    (string)FindResource("Str_DeleteQuestionnaireOfWorkout"), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                App.Db.DeleteQuestionnaires(_user.AccountNo);
                ShowStatus((string)FindResource("Str_DeleteQuestionnaireOfWorkout"));
            }
        }

        private void ShowStatus(string text)
        {
            StatusText.Text = text;
            StatusText.Visibility = Visibility.Visible;
        }
    }
}
