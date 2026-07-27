using System;
using System.Windows;
using BodyComposition.App.Models;
using BodyComposition.App.Services;
using BodyComposition.App.Views;

namespace BodyComposition.App
{
    public partial class App : Application
    {
        public static LocalizationService Localization { get; private set; }
        public static LicenseService License { get; private set; }
        public static DatabaseService Db { get; private set; }
        public static UserProfile CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global crash guard so the app never silently dies like the old one used to.
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
                LogFatal(args.ExceptionObject as Exception);
            DispatcherUnhandledException += (s, args) =>
            {
                LogFatal(args.Exception);
                args.Handled = true;
                MessageBox.Show(args.Exception.Message, "Unexpected error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            };

            Localization = new LocalizationService();
            Localization.ApplyLanguage(Localization.CurrentLanguage); // default fa

            License = new LicenseService();
            Db = new DatabaseService();
            SeedDefaultAdminIfNeeded();

            // Step 1 (required by product owner): license must be validated
            // online, immediately, before anything else in the app is usable.
            var licenseWindow = new LicenseWindow();
            var ok = licenseWindow.ShowDialog();

            if (ok != true)
            {
                Shutdown();
                return;
            }

            // Step 2: normal login screen (User ID / Password / Registration /
            // Visitor Mode / Admin Mode) - functionally identical to v4.11.
            var login = new LoginWindow();
            MainWindow = login;
            login.Show();
        }

        private static void SeedDefaultAdminIfNeeded()
        {
            if (Db.GetAllUsers().Count > 0) return;
            Db.CreateUser(new UserProfile
            {
                Name = "admin",
                Sex = Gender.Male,
                DailyPhysicalLabor = ActivityLevel.Medium,
                Race = Race.Asian,
                Birthday = new DateTime(1990, 1, 1),
                HeightCm = 175,
                IsAdmin = true
            }, "admin123");
        }

        private static void LogFatal(Exception ex)
        {
            try
            {
                System.IO.Directory.CreateDirectory("logs");
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine("logs", "crash.log"),
                    $"{DateTime.Now:u}  {ex}\n\n");
            }
            catch { /* never let logging itself crash the app */ }
        }
    }
}
