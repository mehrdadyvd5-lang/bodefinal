using System;
using System.Collections.Generic;

namespace BodyComposition.App.Models
{
    /// <summary>One full measurement (what the analyzer reports + derived scores).</summary>
    public class BodyMetrics
    {
        public int Id { get; set; }
        public int AccountNo { get; set; }
        public DateTime MeasuredAt { get; set; } = DateTime.Now;

        public double WeightKg { get; set; }
        public double Bmi { get; set; }
        public double Tbf { get; set; }   // Total Body Fat %
        public double Vfi { get; set; }   // Visceral Fat Index
        public double Tbw { get; set; }   // Total Body Water %
        public double Sm { get; set; }    // Skeletal Muscle %
        public double Bmc { get; set; }   // Bone Mineral Content kg
        public double Bmr { get; set; }   // Basal Metabolism Rate kcal/day

        public double UpperBalance { get; set; }
        public double TotalBalance { get; set; }
        public double LowerBalance { get; set; }

        public double TotalScore { get; set; }
        public double BioAge { get; set; }

        public double FatMassKg { get; set; }
        public double FatMassIndex { get; set; }
        public double FatFreeMassKg { get; set; }
        public double FatFreeMassIndex { get; set; }
        public double FatToSmRatio { get; set; }

        public string BodyTypeEvaluation { get; set; }
        public string HealthAdviceText { get; set; }
        public string HealthWarningText { get; set; }
    }

    /// <summary>Where Lower/Low/Normal/High/Higher bands sit for a given indicator.
    /// Values are illustrative starting points; replace with the analyzer's own
    /// calibration tables once available (see README - device protocol item).</summary>
    public static class IdealRanges
    {
        public class Band
        {
            public double Min, Max;
            public string Label;
            public Band(double min, double max, string label) { Min = min; Max = max; Label = label; }
        }

        public static List<Band> ForIndicator(string key, Gender gender)
        {
            switch (key)
            {
                case "BMI":
                    return new List<Band> {
                        new Band(0, 18.5, "Low"), new Band(18.5, 23.9, "Normal"),
                        new Band(23.9, 27, "High"), new Band(27, 40, "Higher") };
                case "TBF":
                    return gender == Gender.Male
                        ? new List<Band> { new Band(0,10,"Low"), new Band(10,20,"Normal"), new Band(20,25,"High"), new Band(25,50,"Higher") }
                        : new List<Band> { new Band(0,18,"Low"), new Band(18,28,"Normal"), new Band(28,33,"High"), new Band(33,55,"Higher") };
                case "VFI":
                    return new List<Band> { new Band(0,2.1,"Low"), new Band(2.1,9,"Normal"), new Band(9,15,"High"), new Band(15,30,"Higher") };
                case "TBW":
                    return new List<Band> { new Band(0,45,"Low"), new Band(45,65,"Normal"), new Band(65,75,"High"), new Band(75,90,"Higher") };
                case "SM":
                    return new List<Band> { new Band(0,33,"Low"), new Band(33,45,"Normal"), new Band(45,55,"High"), new Band(55,70,"Higher") };
                case "BMC":
                    return new List<Band> { new Band(0,2.5,"Low"), new Band(2.5,3.8,"Normal"), new Band(3.8,4.5,"High"), new Band(4.5,6,"Higher") };
                case "BMR":
                    return new List<Band> { new Band(0,1300,"Low"), new Band(1300,1800,"Normal"), new Band(1800,2200,"High"), new Band(2200,3000,"Higher") };
                default:
                    return new List<Band> { new Band(0,100,"Normal") };
            }
        }
    }
}
