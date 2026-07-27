using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views
{
    public partial class QuestionnaireWindow : Window
    {
        private readonly UserProfile _user;
        private RadioButton[] _q1, _q2, _q3, _q4;
        private CheckBox[] _q5Boxes, _q6Boxes;
        private const int MaxDiseases = 2;
        private const int MaxSports = 6;

        private static readonly string[] Q1Options = { "one year ago or never", "half a year ago", "3 months ago", "never stop workout in recent 3 months" };
        private static readonly string[] Q2Options = { "zero-once", "twice", "3 times", "more than 4 times" };
        private static readonly string[] Q3Options = { "No workout", "Less than 30 minutes", "30-60 minute", "more than 60 minutes" };
        private static readonly string[] Q4Options = { "keep fit", "lose weight", "enhance muscle", "a hobby" };
        private static readonly string[] Q5Options = { "no", "diabetes", "fatty liver", "heart disease", "high blood pressure", "osteoporosis" };
        private static readonly string[] Q6Options = {
            "basketball", "badminton", "football", "tennis", "walking", "volleyball",
            "table tennis", "fast walking", "bowling", "golf", "bicycle", "handball", "instrument training",
            "dynamic bicycle", "rope skipping", "yoga", "martial arts", "swimming (1.5km/h)", "swimming (3.5km/h)", "climbing stairs"
        };

        public QuestionnaireWindow(UserProfile user)
        {
            InitializeComponent();
            FlowDirection = App.Localization.FlowDirection;
            _user = user;
            Build();
        }

        private void Build()
        {
            _q1 = AddSingleChoice((string)FindResource("Str_Q1"), Q1Options);
            _q2 = AddSingleChoice((string)FindResource("Str_Q2"), Q2Options);
            _q3 = AddSingleChoice((string)FindResource("Str_Q3"), Q3Options);
            _q4 = AddSingleChoice((string)FindResource("Str_Q4"), Q4Options);
            _q5Boxes = AddMultiChoice((string)FindResource("Str_Q5"), Q5Options, MaxDiseases);
            _q6Boxes = AddMultiChoice((string)FindResource("Str_Q6"), Q6Options, MaxSports);
        }

        private RadioButton[] AddSingleChoice(string question, string[] options)
        {
            QuestionsPanel.Children.Add(new TextBlock
            {
                Text = question, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black, Margin = new Thickness(0, 10, 0, 6)
            });
            var group = System.Guid.NewGuid().ToString();
            var wrap = new WrapPanel();
            var buttons = new RadioButton[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var rb = new RadioButton
                {
                    Content = options[i], GroupName = group,
                    Margin = new Thickness(0, 0, 20, 6), Foreground = Brushes.Black
                };
                buttons[i] = rb;
                wrap.Children.Add(rb);
            }
            QuestionsPanel.Children.Add(wrap);
            return buttons;
        }

        private CheckBox[] AddMultiChoice(string question, string[] options, int max)
        {
            QuestionsPanel.Children.Add(new TextBlock
            {
                Text = question, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.Black, Margin = new Thickness(0, 10, 0, 6)
            });
            var wrap = new WrapPanel();
            var boxes = new CheckBox[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var cb = new CheckBox { Content = options[i], Margin = new Thickness(0, 0, 20, 6), Foreground = Brushes.Black };
                cb.Checked += (s, e) =>
                {
                    if (boxes.Count(b => b.IsChecked == true) > max)
                        ((CheckBox)s).IsChecked = false;
                };
                boxes[i] = cb;
                wrap.Children.Add(cb);
            }
            QuestionsPanel.Children.Add(wrap);
            return boxes;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var answer = new QuestionnaireAnswer
            {
                AccountNo = _user.AccountNo,
                WorkoutHistory = SelectedOf(_q1, Q1Options),
                WorkoutFrequency = SelectedOf(_q2, Q2Options),
                WorkoutDuration = SelectedOf(_q3, Q3Options),
                WorkoutGoal = SelectedOf(_q4, Q4Options),
                Diseases = SelectedListOf(_q5Boxes, Q5Options),
                InterestedSports = SelectedListOf(_q6Boxes, Q6Options)
            };
            App.Db.SaveQuestionnaire(answer);
            DialogResult = true;
            Close();
        }

        private static string SelectedOf(RadioButton[] group, string[] options)
        {
            for (int i = 0; i < group.Length; i++)
                if (group[i].IsChecked == true) return options[i];
            return "";
        }

        private static List<string> SelectedListOf(CheckBox[] group, string[] options)
        {
            var list = new List<string>();
            for (int i = 0; i < group.Length; i++)
                if (group[i].IsChecked == true) list.Add(options[i]);
            return list;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
