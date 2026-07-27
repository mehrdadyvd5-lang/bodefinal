using System;
using BodyComposition.App.Models;

namespace BodyComposition.App.Services
{
    /// <summary>
    /// Produces a full BodyMetrics reading from weight/height/age/gender using
    /// well known approximation formulas. This exists so every screen (report,
    /// history, charts) has real numbers to show before the analyzer hardware
    /// arrives. Swap Calculate() for a real parser of the device's Bluetooth
    /// payload later - the rest of the app (UI, storage, report) will not need to change.
    /// </summary>
    public static class BodyMetricsCalculator
    {
        public static BodyMetrics Calculate(double weightKg, double heightCm, int age, Gender gender, int accountNo)
        {
            var heightM = heightCm / 100.0;
            var bmi = weightKg / (heightM * heightM);

            // Deurenberg body fat % estimate
            var genderTerm = gender == Gender.Male ? 1 : 0;
            var tbf = (1.20 * bmi) + (0.23 * age) - (10.8 * genderTerm) - 5.4;
            tbf = Clamp(tbf, 3, 55);

            var vfi = Clamp((tbf / 2.2) + (age / 12.0) - (gender == Gender.Male ? 2 : 4), 0.5, 25);
            var tbw = Clamp(gender == Gender.Male ? 60 - (tbf * 0.4) : 50 - (tbf * 0.4), 35, 75);
            var sm = Clamp(gender == Gender.Male ? 45 - (tbf * 0.35) : 36 - (tbf * 0.35), 20, 55);
            var bmc = Clamp(weightKg * 0.04, 1.5, 5.5);
            var bmr = gender == Gender.Male
                ? (10 * weightKg) + (6.25 * heightCm) - (5 * age) + 5
                : (10 * weightKg) + (6.25 * heightCm) - (5 * age) - 161;

            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var upperBalance = Math.Round(0.95 + rnd.NextDouble() * 0.1, 3);
            var lowerBalance = Math.Round(0.95 + rnd.NextDouble() * 0.1, 3);
            var totalBalance = Math.Round((upperBalance + lowerBalance) / 2, 3);

            var totalScore = Clamp(100 - Math.Abs(bmi - 21.5) * 3 - Math.Abs(tbf - (gender == Gender.Male ? 17 : 24)) * 1.2, 20, 99);
            var bioAge = Clamp(age + (bmi - 21.5) * 0.8 + (tbf - (gender == Gender.Male ? 17 : 24)) * 0.3, 15, 90);

            var fatMass = weightKg * tbf / 100.0;
            var fatFreeMass = weightKg - fatMass;
            var fatMassIndex = fatMass / (heightM * heightM);
            var fatFreeMassIndex = fatFreeMass / (heightM * heightM);
            var fatToSm = sm > 0 ? Math.Round(fatMass / (weightKg * sm / 100.0), 2) : 0;

            return new BodyMetrics
            {
                AccountNo = accountNo,
                MeasuredAt = DateTime.Now,
                WeightKg = Math.Round(weightKg, 1),
                Bmi = Math.Round(bmi, 1),
                Tbf = Math.Round(tbf, 1),
                Vfi = Math.Round(vfi, 1),
                Tbw = Math.Round(tbw, 1),
                Sm = Math.Round(sm, 1),
                Bmc = Math.Round(bmc, 1),
                Bmr = Math.Round(bmr, 0),
                UpperBalance = upperBalance,
                TotalBalance = totalBalance,
                LowerBalance = lowerBalance,
                TotalScore = Math.Round(totalScore, 1),
                BioAge = Math.Round(bioAge, 0),
                FatMassKg = Math.Round(fatMass, 2),
                FatMassIndex = Math.Round(fatMassIndex, 2),
                FatFreeMassKg = Math.Round(fatFreeMass, 2),
                FatFreeMassIndex = Math.Round(fatFreeMassIndex, 2),
                FatToSmRatio = fatToSm,
                BodyTypeEvaluation = BuildBodyTypeEvaluation(tbf, sm, gender),
                HealthAdviceText = BuildHealthAdvice(bmi, tbf, gender),
                HealthWarningText = BuildHealthWarning(vfi)
            };
        }

        private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);

        private static string BuildBodyTypeEvaluation(double tbf, double sm, Gender gender)
        {
            var lowMuscle = sm < (gender == Gender.Male ? 35 : 28);
            return lowMuscle
                ? "Low muscle content caused by too little exercise. Calories can not be consumed effectively which convert to fat. Please increase the amount of exercise, especially those that may strengthen muscle. Aerobic exercise is also helpful. Please avoid high calorie food, such as fried food, fat meat, dessert etc."
                : "Body composition is within a reasonable range. Keep a consistent exercise routine and a balanced diet to maintain it.";
        }

        private static string BuildHealthAdvice(double bmi, double tbf, Gender gender) =>
            bmi > 24
                ? "Weight and body fat are above the ideal range. A gradual, sustainable reduction plan (diet + exercise) is recommended."
                : "Weight and body fat are within or near the ideal range. Focus on maintaining current habits.";

        private static string BuildHealthWarning(double vfi) =>
            vfi > 9
                ? "Warning: visceral fat level is high. There is something wrong with your lipid metabolism, easy to cause fatty liver, high blood lipids. Please lower visceral fat level to the ideal value."
                : "Visceral fat level is within the ideal range.";
    }
}
