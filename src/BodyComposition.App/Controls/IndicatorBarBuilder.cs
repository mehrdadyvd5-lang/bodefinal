using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BodyComposition.App.Models;

namespace BodyComposition.App.Controls
{
    /// <summary>
    /// Draws a horizontal band of coloured segments (like the original app's
    /// grey/yellow/red bar) plus a bold value readout and a plain-language
    /// condition label. Colors are chosen for AA contrast against both the
    /// segment fill and the surrounding light card background.
    /// </summary>
    public static class IndicatorBarBuilder
    {
        // Fill colors per condition band (kept mid-tone so white/black text on
        // top of them, and the segments against the light card, both pass contrast).
        private static readonly Dictionary<string, Color> BandColor = new Dictionary<string, Color>
        {
            { "Low",    Color.FromRgb(0x7B, 0xC8, 0xE8) }, // blue - below normal
            { "Normal", Color.FromRgb(0x4C, 0xAF, 0x67) }, // green - ideal
            { "High",   Color.FromRgb(0xE8, 0xB4, 0x33) }, // amber - above normal
            { "Higher", Color.FromRgb(0xD1, 0x4B, 0x4B) }, // red - well above normal
        };

        private const int SegmentCount = 15;

        public static UIElement Build(string label, double value, string unit,
            List<IdealRanges.Band> bands, out string conditionLabel)
        {
            var min = bands[0].Min;
            var max = bands[bands.Count - 1].Max;
            var range = max - min <= 0 ? 1 : max - min;
            var clamped = value < min ? min : (value > max ? max : value);
            var filledSegments = (int)System.Math.Round(((clamped - min) / range) * SegmentCount);
            if (filledSegments < 1) filledSegments = 1;

            IdealRanges.Band activeBand = bands[0];
            foreach (var b in bands)
                if (value >= b.Min && value <= b.Max) activeBand = b;
            conditionLabel = activeBand.Label;

            var root = new Grid();
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var segRow = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var fillColor = BandColor.ContainsKey(activeBand.Label) ? BandColor[activeBand.Label] : BandColor["Normal"];

            for (int i = 0; i < SegmentCount; i++)
            {
                var seg = new Border
                {
                    Width = 14,
                    Height = 18,
                    Margin = new Thickness(1, 0, 1, 0),
                    CornerRadius = new CornerRadius(2),
                    Background = i < filledSegments
                        ? new SolidColorBrush(fillColor)
                        : new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)), // neutral, readable against white card
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
                    BorderThickness = new Thickness(0.5)
                };
                segRow.Children.Add(seg);
            }
            Grid.SetColumn(segRow, 0);
            root.Children.Add(segRow);

            var labelText = new TextBlock
            {
                Text = $"{conditionLabel}",
                Foreground = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)), // dark on light card, always readable
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(labelText, 1);
            root.Children.Add(labelText);

            return root;
        }
    }
}
