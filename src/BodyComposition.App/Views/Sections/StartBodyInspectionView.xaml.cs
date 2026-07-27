using System;
using System.Windows;
using System.Windows.Controls;
using BodyComposition.App.Controls;
using BodyComposition.App.Models;
using BodyComposition.App.Services;

namespace BodyComposition.App.Views.Sections
{
    public partial class StartBodyInspectionView : UserControl
    {
        private UserProfile _user;
        private BodyMetrics _current;
        private TextBox _weightInput;

        public StartBodyInspectionView()
        {
            InitializeComponent();
        }

        public void Load(UserProfile user)
        {
            _user = user;
            DateText.Text = DateTime.Today.ToString("yyyy-MM-dd");
            BuildEmptyPanel();
        }

        /// <summary>Shows a previously saved measurement (read-only view from history).</summary>
        public void LoadExisting(UserProfile user, BodyMetrics m)
        {
            _user = user;
            _current = m;
            DateText.Text = m.MeasuredAt.ToString("yyyy-MM-dd");
            RenderMetrics(m);
            SaveButton.IsEnabled = false;
            PrintButton.IsEnabled = true;
            StartButton.IsEnabled = false;
        }

        private void BuildEmptyPanel()
        {
            MetricsPanel.Children.Clear();
            _weightInput = new TextBox
            {
                Style = (Style)FindResource("GlassTextBox"),
                Margin = new Thickness(0, 0, 0, 10),
                Width = 120,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            row.Children.Add(new TextBlock
            {
                Text = (string)FindResource("Str_Weight"),
                FontSize = 13, FontWeight = FontWeights.Bold,
                Foreground = (System.Windows.Media.Brush)FindResource("BrandGreenDarkBrush"),
                VerticalAlignment = VerticalAlignment.Center, Width = 140
            });
            row.Children.Add(_weightInput);
            MetricsPanel.Children.Add(row);

            TotalScoreText.Text = "-"; BioAgeText.Text = "-";
            UpperBalanceText.Text = "-"; TotalBalanceText.Text = "-"; LowerBalanceText.Text = "-";
            BodyTypeText.Text = ""; HealthAdviceTextBlock.Text = ""; HealthWarningTextBlock.Text = "";
            SaveButton.IsEnabled = false;
            PrintButton.IsEnabled = false;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_weightInput == null || !double.TryParse(_weightInput.Text, out var weight) || weight <= 0)
            {
                StatusText.Visibility = Visibility.Visible;
                StatusText.Foreground = System.Windows.Media.Brushes.DarkRed;
                StatusText.Text = (string)FindResource("Str_RequiredFieldsMissing");
                return;
            }

            // Once the analyzer hardware is connected, replace this call with
            // the parsed reading from BluetoothConnectWindow instead.
            _current = BodyMetricsCalculator.Calculate(weight, _user.HeightCm, _user.Age, _user.Sex, _user.AccountNo);
            RenderMetrics(_current);

            SaveButton.IsEnabled = true;
            PrintButton.IsEnabled = true;
            StatusText.Visibility = Visibility.Collapsed;
        }

        private void RenderMetrics(BodyMetrics m)
        {
            MetricsPanel.Children.Clear();

            AddMetricRow((string)FindResource("Str_Weight"), m.WeightKg, "kg", "BMI_WEIGHT_PLACEHOLDER");
            AddMetricRow("BMI", m.Bmi, "", "BMI");
            AddMetricRow("TBF%", m.Tbf, "%", "TBF");
            AddMetricRow("VFI", m.Vfi, "", "VFI");
            AddMetricRow("TBW%", m.Tbw, "%", "TBW");
            AddMetricRow("SM%", m.Sm, "%", "SM");
            AddMetricRow("BMC(kg)", m.Bmc, "kg", "BMC");
            AddMetricRow("BMR(Kcal/day)", m.Bmr, "kcal", "BMR");

            UpperBalanceText.Text = m.UpperBalance.ToString("0.000");
            TotalBalanceText.Text = m.TotalBalance.ToString("0.000");
            LowerBalanceText.Text = m.LowerBalance.ToString("0.000");
            TotalScoreText.Text = m.TotalScore.ToString("0.0");
            BioAgeText.Text = m.BioAge.ToString("0");

            BodyTypeText.Text = m.BodyTypeEvaluation;
            HealthAdviceTextBlock.Text = m.HealthAdviceText;
            HealthWarningTextBlock.Text = m.HealthWarningText;
        }

        private void AddMetricRow(string label, double value, string unit, string bandKey)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = new TextBlock
            {
                Text = label, FontSize = 13, FontWeight = FontWeights.SemiBold,
                Foreground = (System.Windows.Media.Brush)FindResource("BrandGreenDarkBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(lbl, 0);
            row.Children.Add(lbl);

            var val = new TextBlock
            {
                Text = value.ToString("0.0"), FontSize = 14, FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black, VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(val, 1);
            row.Children.Add(val);

            if (bandKey != "BMI_WEIGHT_PLACEHOLDER")
            {
                var bands = IdealRanges.ForIndicator(bandKey, _user.Sex);
                var bar = IndicatorBarBuilder.Build(label, value, unit, bands, out _);
                Grid.SetColumn(bar, 2);
                row.Children.Add(bar);
            }

            MetricsPanel.Children.Add(row);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            App.Db.SaveMeasurement(_current);
            StatusText.Visibility = Visibility.Visible;
            StatusText.Foreground = (System.Windows.Media.Brush)FindResource("BrandGreenDarkBrush");
            StatusText.Text = (string)FindResource("Str_MeasurementSaved");
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (_current == null) return;
            var report = new ReportWindow(_user, _current);
            report.ShowDialog();
        }
    }
}
