using System.Windows.Controls;
using BodyComposition.App.Models;

namespace BodyComposition.App.Views.Sections
{
    public partial class ExerciseSuggestionView : UserControl
    {
        private UserProfile _user;

        public ExerciseSuggestionView()
        {
            InitializeComponent();
        }

        public void Load(UserProfile user)
        {
            _user = user;
            SummaryText.Text = "Fill in the questionnaire so a personalised exercise suggestion can be generated from your goals, workout history and health conditions.";
        }

        private void OpenQuestionnaire_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var q = new QuestionnaireWindow(_user);
            q.ShowDialog();
        }
    }
}
