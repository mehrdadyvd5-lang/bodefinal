using System.Windows;
using System.Windows.Controls;
using BodyComposition.App.Controls;
using BodyComposition.App.Models;
using BodyComposition.App.Services;

namespace BodyComposition.App.Views
{
    public partial class VisitorModeWindow : Window
    {
        private BodyMetrics _current;
        private Gender _gender;

        public VisitorModeWindow()
        {
            InitializeComponent();
            FlowDirection = App.Localization.FlowDirection;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            if (!double.TryParse(AgeBox.Text, out var age) ||
                !double.TryParse(HeightBox.Text, out var height) ||
                !double.TryParse(WeightBox.Text, out var weight) ||
                age <= 0 || height <= 0 || weight <= 0)
            {
                ErrorText.Text = (string)FindResource("Str_RequiredFieldsMissing");
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            _gender = SexCombo.SelectedIndex == 1 ? Gender.Female : Gender.Male;
            _current = BodyMetricsCalculator.Calculate(weight, height, (int)age, _gender, 0);
            RenderMetrics();
            PrintButton.IsEnabled = true;
        }

        private void RenderMetrics()
        {
            MetricsPanel.Children.Clear();
            AddRow("Weight(kg)", _current.WeightKg, "");
            AddRow("BMI", _current.Bmi, "BMI");
            AddRow("TBF%", _current.Tbf, "TBF");
            AddRow("VFI", _current.Vfi, "VFI");
            AddRow("TBW%", _current.Tbw, "TBW");
            AddRow("SM%", _current.Sm, "SM");
            AddRow("BMC(kg)", _current.Bmc, "BMC");
            AddRow("BMR(Kcal/day)", _current.Bmr, "BMR");
            BodyTypeText.Text = _current.BodyTypeEvaluation;
            HealthWarningText.Text = _current.HealthWarningText;
        }

        private void AddRow(string label, double value, string bandKey)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock { Text = label, FontWeight = System.Windows.FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(lbl, 0); row.Children.Add(lbl);
            var val = new TextBlock { Text = value.ToString("0.0"), FontWeight = System.Windows.FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(val, 1); row.Children.Add(val);

            if (!string.IsNullOrEmpty(bandKey))
            {
                var bands = IdealRanges.ForIndicator(bandKey, _gender);
                var bar = IndicatorBarBuilder.Build(label, value, "", bands, out _);
                Grid.SetColumn(bar, 2); row.Children.Add(bar);
            }
            MetricsPanel.Children.Add(row);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            var visitorProfile = new UserProfile
            {
                Name = string.IsNullOrWhiteSpace(NameBox.Text) ? "Visitor" : NameBox.Text,
                Sex = _gender,
                HeightCm = _current.WeightKg > 0 ? double.Parse(HeightBox.Text) : 0
            };
            new ReportWindow(visitorProfile, _current).ShowDialog();
        }
    }
}
