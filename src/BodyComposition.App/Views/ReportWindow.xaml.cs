using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using BodyComposition.App.Controls;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views
{
    public partial class ReportWindow : Window
    {
        public ReportWindow(UserProfile user, BodyMetrics m)
        {
            InitializeComponent();
            FlowDirection = App.Localization.FlowDirection;

            NameText.Text = user.Name;
            GenderText.Text = user.Sex.ToString();
            AgeText.Text = user.Age.ToString();
            HeightTimeText.Text = $"{user.HeightCm} cm  —  {m.MeasuredAt:yyyy-MM-dd HH:mm}";

            TotalScoreText.Text = m.TotalScore.ToString("0.0");
            BioAgeText.Text = m.BioAge.ToString("0");
            BodyTypeText.Text = m.BodyTypeEvaluation;
            HealthAdviceText.Text = m.HealthAdviceText;
            HealthWarningText.Text = m.HealthWarningText;

            AddIndicator("Weight(kg)", m.WeightKg, "");
            AddIndicator("BMI", m.Bmi, "BMI");
            AddIndicator("TBF%", m.Tbf, "TBF");
            AddIndicator("VFI", m.Vfi, "VFI");
            AddIndicator("TBW%", m.Tbw, "TBW");
            AddIndicator("SM%", m.Sm, "SM");
            AddIndicator("BMC(kg)", m.Bmc, "BMC");
            AddIndicator("BMR(Kcal/day)", m.Bmr, "BMR");

            void AddIndicator(string label, double value, string bandKey)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var lbl = new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Foreground = System.Windows.Media.Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(lbl, 0);
                row.Children.Add(lbl);

                var val = new TextBlock { Text = value.ToString("0.0"), FontWeight = FontWeights.Bold, Foreground = System.Windows.Media.Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(val, 1);
                row.Children.Add(val);

                if (!string.IsNullOrEmpty(bandKey))
                {
                    var bands = IdealRanges.ForIndicator(bandKey, user.Sex);
                    var bar = IndicatorBarBuilder.Build(label, value, "", bands, out _);
                    Grid.SetColumn(bar, 2);
                    row.Children.Add(bar);
                }
                IndicatorsPanel.Children.Add(row);
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                dlg.PrintVisual(ReportRoot, "Human Body Composition Report");
            }
        }
    }
}
