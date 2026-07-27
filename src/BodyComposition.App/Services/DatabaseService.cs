using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BodyComposition.App.Models;

namespace BodyComposition.App.Services
{
    /// <summary>
    /// All local, offline storage. No data ever leaves the machine here -
    /// only the license check (LicenseService) talks to the network.
    /// </summary>
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HumanBodyCompositionMonitor");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "data.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
            EnsureSchema();
        }

        private SQLiteConnection Open()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private void EnsureSchema()
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    AccountNo INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Sex TEXT NOT NULL,
    ActivityLevel TEXT NOT NULL,
    Race TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Birthday TEXT NOT NULL,
    HeightCm REAL NOT NULL,
    TelMobile TEXT,
    QqMsn TEXT,
    Address TEXT,
    PortraitPath TEXT,
    IsAdmin INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Measurements (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AccountNo INTEGER NOT NULL,
    MeasuredAt TEXT NOT NULL,
    WeightKg REAL, Bmi REAL, Tbf REAL, Vfi REAL, Tbw REAL, Sm REAL, Bmc REAL, Bmr REAL,
    UpperBalance REAL, TotalBalance REAL, LowerBalance REAL,
    TotalScore REAL, BioAge REAL,
    FatMassKg REAL, FatMassIndex REAL, FatFreeMassKg REAL, FatFreeMassIndex REAL, FatToSmRatio REAL,
    BodyTypeEvaluation TEXT, HealthAdviceText TEXT, HealthWarningText TEXT,
    FOREIGN KEY(AccountNo) REFERENCES Users(AccountNo)
);
CREATE TABLE IF NOT EXISTS Questionnaires (
    AccountNo INTEGER NOT NULL,
    SavedAt TEXT NOT NULL,
    WorkoutHistory TEXT, WorkoutFrequency TEXT, WorkoutDuration TEXT, WorkoutGoal TEXT,
    Diseases TEXT, InterestedSports TEXT
);
CREATE TABLE IF NOT EXISTS Settings (
    Key TEXT PRIMARY KEY,
    Value TEXT
);";
                cmd.ExecuteNonQuery();
            }
        }

        // ---------------- Users ----------------

        public static string HashPassword(string plain)
        {
            using (var sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(plain ?? "")));
        }

        public int CreateUser(UserProfile u, string plainPassword)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Users
                    (Name, Sex, ActivityLevel, Race, PasswordHash, Birthday, HeightCm, TelMobile, QqMsn, Address, PortraitPath, IsAdmin)
                    VALUES (@n,@s,@a,@r,@p,@b,@h,@t,@q,@ad,@pp,@ia); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@n", u.Name);
                cmd.Parameters.AddWithValue("@s", u.Sex.ToString());
                cmd.Parameters.AddWithValue("@a", u.DailyPhysicalLabor.ToString());
                cmd.Parameters.AddWithValue("@r", u.Race.ToString());
                cmd.Parameters.AddWithValue("@p", HashPassword(plainPassword));
                cmd.Parameters.AddWithValue("@b", u.Birthday.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@h", u.HeightCm);
                cmd.Parameters.AddWithValue("@t", (object)u.TelMobile ?? "");
                cmd.Parameters.AddWithValue("@q", (object)u.QqMsn ?? "");
                cmd.Parameters.AddWithValue("@ad", (object)u.Address ?? "");
                cmd.Parameters.AddWithValue("@pp", (object)u.PortraitPath ?? "");
                cmd.Parameters.AddWithValue("@ia", u.IsAdmin ? 1 : 0);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public UserProfile TryLogin(string nameOrAccount, string plainPassword)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT * FROM Users WHERE (Name = @n OR CAST(AccountNo AS TEXT) = @n) AND PasswordHash = @p";
                cmd.Parameters.AddWithValue("@n", nameOrAccount);
                cmd.Parameters.AddWithValue("@p", HashPassword(plainPassword));
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) return ReadUser(r);
                }
            }
            return null;
        }

        public List<UserProfile> GetAllUsers()
        {
            var list = new List<UserProfile>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Users ORDER BY AccountNo";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(ReadUser(r));
            }
            return list;
        }

        public void DeleteUser(int accountNo)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Users WHERE AccountNo=@a; DELETE FROM Measurements WHERE AccountNo=@a; DELETE FROM Questionnaires WHERE AccountNo=@a;";
                cmd.Parameters.AddWithValue("@a", accountNo);
                cmd.ExecuteNonQuery();
            }
        }

        public void ClearPassword(int accountNo, string newPlainPassword)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Users SET PasswordHash=@p WHERE AccountNo=@a";
                cmd.Parameters.AddWithValue("@p", HashPassword(newPlainPassword));
                cmd.Parameters.AddWithValue("@a", accountNo);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateUser(UserProfile u)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE Users SET Name=@n, Sex=@s, ActivityLevel=@a, Race=@r,
                    Birthday=@b, HeightCm=@h, TelMobile=@t, QqMsn=@q, Address=@ad, PortraitPath=@pp
                    WHERE AccountNo=@acc";
                cmd.Parameters.AddWithValue("@n", u.Name);
                cmd.Parameters.AddWithValue("@s", u.Sex.ToString());
                cmd.Parameters.AddWithValue("@a", u.DailyPhysicalLabor.ToString());
                cmd.Parameters.AddWithValue("@r", u.Race.ToString());
                cmd.Parameters.AddWithValue("@b", u.Birthday.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@h", u.HeightCm);
                cmd.Parameters.AddWithValue("@t", (object)u.TelMobile ?? "");
                cmd.Parameters.AddWithValue("@q", (object)u.QqMsn ?? "");
                cmd.Parameters.AddWithValue("@ad", (object)u.Address ?? "");
                cmd.Parameters.AddWithValue("@pp", (object)u.PortraitPath ?? "");
                cmd.Parameters.AddWithValue("@acc", u.AccountNo);
                cmd.ExecuteNonQuery();
            }
        }

        private static UserProfile ReadUser(SQLiteDataReader r) => new UserProfile
        {
            AccountNo = Convert.ToInt32(r["AccountNo"]),
            Name = r["Name"].ToString(),
            Sex = (Gender)Enum.Parse(typeof(Gender), r["Sex"].ToString()),
            DailyPhysicalLabor = (ActivityLevel)Enum.Parse(typeof(ActivityLevel), r["ActivityLevel"].ToString()),
            Race = (Race)Enum.Parse(typeof(Race), r["Race"].ToString()),
            Birthday = DateTime.Parse(r["Birthday"].ToString()),
            HeightCm = Convert.ToDouble(r["HeightCm"]),
            TelMobile = r["TelMobile"].ToString(),
            QqMsn = r["QqMsn"].ToString(),
            Address = r["Address"].ToString(),
            PortraitPath = r["PortraitPath"].ToString(),
            IsAdmin = Convert.ToInt32(r["IsAdmin"]) == 1,
        };

        // ---------------- Measurements ----------------

        public int SaveMeasurement(BodyMetrics m)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Measurements
                    (AccountNo, MeasuredAt, WeightKg, Bmi, Tbf, Vfi, Tbw, Sm, Bmc, Bmr,
                     UpperBalance, TotalBalance, LowerBalance, TotalScore, BioAge,
                     FatMassKg, FatMassIndex, FatFreeMassKg, FatFreeMassIndex, FatToSmRatio,
                     BodyTypeEvaluation, HealthAdviceText, HealthWarningText)
                    VALUES (@acc,@dt,@w,@bmi,@tbf,@vfi,@tbw,@sm,@bmc,@bmr,@ub,@tb,@lb,@ts,@ba,
                     @fm,@fmi,@ffm,@ffmi,@fsr,@bte,@hat,@hwt); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@acc", m.AccountNo);
                cmd.Parameters.AddWithValue("@dt", m.MeasuredAt.ToString("s"));
                cmd.Parameters.AddWithValue("@w", m.WeightKg);
                cmd.Parameters.AddWithValue("@bmi", m.Bmi);
                cmd.Parameters.AddWithValue("@tbf", m.Tbf);
                cmd.Parameters.AddWithValue("@vfi", m.Vfi);
                cmd.Parameters.AddWithValue("@tbw", m.Tbw);
                cmd.Parameters.AddWithValue("@sm", m.Sm);
                cmd.Parameters.AddWithValue("@bmc", m.Bmc);
                cmd.Parameters.AddWithValue("@bmr", m.Bmr);
                cmd.Parameters.AddWithValue("@ub", m.UpperBalance);
                cmd.Parameters.AddWithValue("@tb", m.TotalBalance);
                cmd.Parameters.AddWithValue("@lb", m.LowerBalance);
                cmd.Parameters.AddWithValue("@ts", m.TotalScore);
                cmd.Parameters.AddWithValue("@ba", m.BioAge);
                cmd.Parameters.AddWithValue("@fm", m.FatMassKg);
                cmd.Parameters.AddWithValue("@fmi", m.FatMassIndex);
                cmd.Parameters.AddWithValue("@ffm", m.FatFreeMassKg);
                cmd.Parameters.AddWithValue("@ffmi", m.FatFreeMassIndex);
                cmd.Parameters.AddWithValue("@fsr", m.FatToSmRatio);
                cmd.Parameters.AddWithValue("@bte", (object)m.BodyTypeEvaluation ?? "");
                cmd.Parameters.AddWithValue("@hat", (object)m.HealthAdviceText ?? "");
                cmd.Parameters.AddWithValue("@hwt", (object)m.HealthWarningText ?? "");
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public List<BodyMetrics> GetMeasurements(int accountNo)
        {
            var list = new List<BodyMetrics>();
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Measurements WHERE AccountNo=@a ORDER BY MeasuredAt DESC";
                cmd.Parameters.AddWithValue("@a", accountNo);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new BodyMetrics
                        {
                            Id = Convert.ToInt32(r["Id"]),
                            AccountNo = accountNo,
                            MeasuredAt = DateTime.Parse(r["MeasuredAt"].ToString()),
                            WeightKg = Convert.ToDouble(r["WeightKg"]),
                            Bmi = Convert.ToDouble(r["Bmi"]),
                            Tbf = Convert.ToDouble(r["Tbf"]),
                            Vfi = Convert.ToDouble(r["Vfi"]),
                            Tbw = Convert.ToDouble(r["Tbw"]),
                            Sm = Convert.ToDouble(r["Sm"]),
                            Bmc = Convert.ToDouble(r["Bmc"]),
                            Bmr = Convert.ToDouble(r["Bmr"]),
                            UpperBalance = Convert.ToDouble(r["UpperBalance"]),
                            TotalBalance = Convert.ToDouble(r["TotalBalance"]),
                            LowerBalance = Convert.ToDouble(r["LowerBalance"]),
                            TotalScore = Convert.ToDouble(r["TotalScore"]),
                            BioAge = Convert.ToDouble(r["BioAge"]),
                            FatMassKg = Convert.ToDouble(r["FatMassKg"]),
                            FatMassIndex = Convert.ToDouble(r["FatMassIndex"]),
                            FatFreeMassKg = Convert.ToDouble(r["FatFreeMassKg"]),
                            FatFreeMassIndex = Convert.ToDouble(r["FatFreeMassIndex"]),
                            FatToSmRatio = Convert.ToDouble(r["FatToSmRatio"]),
                            BodyTypeEvaluation = r["BodyTypeEvaluation"].ToString(),
                            HealthAdviceText = r["HealthAdviceText"].ToString(),
                            HealthWarningText = r["HealthWarningText"].ToString(),
                        });
            }
            return list;
        }

        public void DeleteMeasurement(int id)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Measurements WHERE Id=@id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAllMeasurements(int accountNo)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Measurements WHERE AccountNo=@a";
                cmd.Parameters.AddWithValue("@a", accountNo);
                cmd.ExecuteNonQuery();
            }
        }

        // ---------------- Questionnaire ----------------

        public void SaveQuestionnaire(QuestionnaireAnswer q)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Questionnaires
                    (AccountNo, SavedAt, WorkoutHistory, WorkoutFrequency, WorkoutDuration, WorkoutGoal, Diseases, InterestedSports)
                    VALUES (@a,@dt,@h,@f,@d,@g,@dis,@sp)";
                cmd.Parameters.AddWithValue("@a", q.AccountNo);
                cmd.Parameters.AddWithValue("@dt", q.SavedAt.ToString("s"));
                cmd.Parameters.AddWithValue("@h", q.WorkoutHistory ?? "");
                cmd.Parameters.AddWithValue("@f", q.WorkoutFrequency ?? "");
                cmd.Parameters.AddWithValue("@d", q.WorkoutDuration ?? "");
                cmd.Parameters.AddWithValue("@g", q.WorkoutGoal ?? "");
                cmd.Parameters.AddWithValue("@dis", string.Join(",", q.Diseases));
                cmd.Parameters.AddWithValue("@sp", string.Join(",", q.InterestedSports));
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteQuestionnaires(int accountNo)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Questionnaires WHERE AccountNo=@a";
                cmd.Parameters.AddWithValue("@a", accountNo);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
