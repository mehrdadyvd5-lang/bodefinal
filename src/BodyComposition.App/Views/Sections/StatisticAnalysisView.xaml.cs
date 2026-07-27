using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views.Sections
{
    public partial class StatisticAnalysisView : UserControl
    {
        private UserProfile _user;

        public StatisticAnalysisView()
        {
            InitializeComponent();
            SizeChanged += (s, e) => Refresh();
        }

        public void Load(UserProfile user)
        {
            _user = user;
            Refresh();
        }

        public void Refresh()
        {
            if (_user == null) return;
            var records = App.Db.GetMeasurements(_user.AccountNo).OrderBy(r => r.MeasuredAt).ToList();

            DrawChart(records);
            BuildHistory(records);
        }

        private void DrawChart(System.Collections.Generic.List<BodyMetrics> records)
        {
            ChartCanvas.Children.Clear();
            var w = ChartCanvas.ActualWidth > 20 ? ChartCanvas.ActualWidth : 600;
            var h = ChartCanvas.ActualHeight > 20 ? ChartCanvas.ActualHeight : 200;

            NoDataText.Visibility = records.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            if (records.Count == 0) return;

            var max = records.Max(r => r.Bmi) + 2;
            var min = Math.Max(0, records.Min(r => r.Bmi) - 2);
            var range = max - min <= 0 ? 1 : max - min;

            var points = new PointCollection();
            for (int i = 0; i < records.Count; i++)
            {
                var x = records.Count == 1 ? w / 2 : (i / (double)(records.Count - 1)) * (w - 30) + 15;
                var y = h - ((records[i].Bmi - min) / range) * (h - 20) - 10;
                points.Add(new Point(x, y));
            }

            var poly = new Polyline
            {
                Points = points,
                Stroke = new SolidColorBrush(Color.FromRgb(0x0E, 0x7A, 0x3B)),
                StrokeThickness = 2.5
            };
            ChartCanvas.Children.Add(poly);

            foreach (var p in points)
            {
                var dot = new Ellipse
                {
                    Width = 8, Height = 8,
                    Fill = new SolidColorBrush(Color.FromRgb(0x2F, 0xAE, 0x60)),
                    Stroke = Brushes.White, StrokeThickness = 1.2
                };
                Canvas.SetLeft(dot, p.X - 4);
                Canvas.SetTop(dot, p.Y - 4);
                ChartCanvas.Children.Add(dot);
            }
        }

        private void BuildHistory(System.Collections.Generic.List<BodyMetrics> records)
        {
            HistoryList.Items.Clear();
            foreach (var r in records.OrderByDescending(x => x.MeasuredAt))
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var date = new TextBlock { Text = r.MeasuredAt.ToString("yyyy-MM-dd"), Foreground = Brushes.Black, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(date, 0); row.Children.Add(date);

                var bmi = new TextBlock { Text = $"BMI {r.Bmi:0.0}", Foreground = Brushes.Black, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(bmi, 1); row.Children.Add(bmi);

                var showBtn = new Button
                {
                    Content = (string)FindResource("Str_ShowReport"),
                    Style = (Style)FindResource("PillButton"),
                    Background = new SolidColorBrush(Color.FromRgb(0xB5, 0x52, 0x9E)),
                    Padding = new Thickness(12, 4, 12, 4), Margin = new Thickness(0, 0, 6, 0),
                    Tag = r
                };
                showBtn.Click += (s, e) => new ReportWindow(_user, (BodyMetrics)((Button)s).Tag).ShowDialog();
                Grid.SetColumn(showBtn, 3); row.Children.Add(showBtn);

                var delBtn = new Button
                {
                    Content = (string)FindResource("Str_Delete"),
                    Style = (Style)FindResource("PillButton"),
                    Background = new SolidColorBrush(Color.FromRgb(0xA8, 0x36, 0x36)),
                    Padding = new Thickness(12, 4, 12, 4),
                    Tag = r
                };
                delBtn.Click += (s, e) =>
                {
                    var rec = (BodyMetrics)((Button)s).Tag;
                    App.Db.DeleteMeasurement(rec.Id);
                    Refresh();
                };
                Grid.SetColumn(delBtn, 4); row.Children.Add(delBtn);

                HistoryList.Items.Add(row);
            }
        }
    }
}
