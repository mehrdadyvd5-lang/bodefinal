using System;

namespace BodyComposition.App.Models
{
    public enum Gender { Male, Female }
    public enum ActivityLevel { Low, Medium, High }
    public enum Race { Asian, Caucasian, African, Other }

    public class UserProfile
    {
        public int AccountNo { get; set; }
        public string Name { get; set; }
        public Gender Sex { get; set; } = Gender.Male;
        public ActivityLevel DailyPhysicalLabor { get; set; } = ActivityLevel.Medium;
        public Race Race { get; set; } = Race.Asian;
        public string PasswordHash { get; set; }
        public DateTime Birthday { get; set; } = new DateTime(2000, 1, 1);
        public double HeightCm { get; set; }
        public string TelMobile { get; set; }
        public string QqMsn { get; set; }
        public string Address { get; set; }
        public string PortraitPath { get; set; }
        public bool IsAdmin { get; set; }

        public int Age
        {
            get
            {
                var now = DateTime.Today;
                var age = now.Year - Birthday.Year;
                if (Birthday.Date > now.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
