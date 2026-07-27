using System;
using System.Linq;
using System.Windows;

namespace BodyComposition.App.Services
{
    public enum AppLanguage { Fa, En }

    /// <summary>
    /// Swaps the active Strings.*.xaml dictionary at runtime so every window
    /// (bound via {DynamicResource Str_Xxx}) updates instantly without a restart.
    /// Also exposes the correct FlowDirection so Persian renders RTL and
    /// English renders LTR.
    /// </summary>
    public class LocalizationService
    {
        public AppLanguage CurrentLanguage { get; private set; } = AppLanguage.Fa;

        public FlowDirection FlowDirection =>
            CurrentLanguage == AppLanguage.Fa ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        public event Action LanguageChanged;

        public void ToggleLanguage()
        {
            ApplyLanguage(CurrentLanguage == AppLanguage.Fa ? AppLanguage.En : AppLanguage.Fa);
        }

        public void ApplyLanguage(AppLanguage language)
        {
            CurrentLanguage = language;
            string uri = language == AppLanguage.Fa
                ? "Resources/Strings/Strings.fa.xaml"
                : "Resources/Strings/Strings.en.xaml";

            var dict = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };

            var merged = Application.Current.Resources.MergedDictionaries;
            var old = merged.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("Resources/Strings/"));
            if (old != null) merged.Remove(old);
            merged.Add(dict);

            LanguageChanged?.Invoke();
        }
    }
}
