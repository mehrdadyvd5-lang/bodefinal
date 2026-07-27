using System.Collections.Generic;

namespace BodyComposition.App.Models
{
    public class QuestionnaireAnswer
    {
        public int AccountNo { get; set; }
        public System.DateTime SavedAt { get; set; } = System.DateTime.Now;

        public string WorkoutHistory { get; set; }      // Q1 single choice
        public string WorkoutFrequency { get; set; }     // Q2 single choice
        public string WorkoutDuration { get; set; }      // Q3 single choice
        public string WorkoutGoal { get; set; }          // Q4 single choice
        public List<string> Diseases { get; set; } = new List<string>();       // Q5 up to 2
        public List<string> InterestedSports { get; set; } = new List<string>(); // Q6 up to 6
    }
}
