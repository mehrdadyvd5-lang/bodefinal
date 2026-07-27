using System;
using System.Management; // System.Management.dll reference, ships with .NET FX
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BodyComposition.App.Models;
using Newtonsoft.Json;

namespace BodyComposition.App.Services
{
    /// <summary>
    /// Contacts the license API on first run (and can be re-called any time)
    /// to validate the code the customer was given. The endpoint below is a
    /// placeholder - point LicenseApiBaseUrl at your real licensing backend.
    ///
    /// NOTE: this is intentionally simple (single endpoint, machine-bound
    /// activation). It is a starting point we will harden together
    /// (e.g. periodic re-validation, offline grace period policy, etc.)
    /// once the licensing backend itself is decided.
    /// </summary>
    public class LicenseService
    {
        // TODO: replace with the real licensing API once it's ready.
        private const string LicenseApiBaseUrl = "https://license.yourdomain.com/api/v1";

        // TEMPORARY: lets you test the app before the real license server
        // exists. Remove this block once LicenseApiBaseUrl is real.
        private const string DevTestCode = "TEST-TEST-TEST-TEST";

        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(12)
        };

        public async Task<LicenseCheckResult> ActivateAsync(string licenseCode)
        {
            if (string.IsNullOrWhiteSpace(licenseCode))
                return new LicenseCheckResult { IsValid = false, Message = "Empty code" };

            if (licenseCode.Trim().Equals(DevTestCode, StringComparison.OrdinalIgnoreCase))
            {
                CacheLicenseLocally(licenseCode);
                return new LicenseCheckResult { IsValid = true, Message = "Dev test mode - no server contacted." };
            }

            var payload = new
            {
                licenseCode = licenseCode.Trim(),
                machineId = GetMachineFingerprint(),
                appVersion = "5.0.0"
            };

            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await Http.PostAsync($"{LicenseApiBaseUrl}/activate", content)
                    .ConfigureAwait(false);

                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return new LicenseCheckResult
                    {
                        IsValid = false,
                        Message = $"Server rejected the code ({(int)response.StatusCode})."
                    };
                }

                var result = JsonConvert.DeserializeObject<LicenseCheckResult>(body);
                if (result != null && result.IsValid)
                {
                    CacheLicenseLocally(licenseCode);
                }
                return result ?? new LicenseCheckResult { IsValid = false, Message = "Empty server response." };
            }
            catch (TaskCanceledException)
            {
                return new LicenseCheckResult { IsValid = false, NetworkError = true };
            }
            catch (HttpRequestException)
            {
                return new LicenseCheckResult { IsValid = false, NetworkError = true };
            }
        }

        /// <summary>
        /// A hardware fingerprint so one code = one machine, similar to how
        /// the hardware dongle worked in the original product - except this
        /// binds to the PC instead of requiring a physical USB key.
        /// (When the dongle-based flow is added later, this can be combined
        /// with the dongle's serial number instead of / in addition to this.)
        /// </summary>
        public static string GetMachineFingerprint()
        {
            string cpuId = ReadWmi("Win32_Processor", "ProcessorId");
            string boardId = ReadWmi("Win32_BaseBoard", "SerialNumber");
            string diskId = ReadWmi("Win32_DiskDrive", "SerialNumber");

            using (var sha = SHA256.Create())
            {
                var raw = Encoding.UTF8.GetBytes($"{cpuId}|{boardId}|{diskId}");
                var hash = sha.ComputeHash(raw);
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 32);
            }
        }

        private static string ReadWmi(string wmiClass, string property)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var val = obj[property]?.ToString();
                        if (!string.IsNullOrWhiteSpace(val)) return val;
                    }
                }
            }
            catch { /* WMI can be blocked by policy - degrade gracefully */ }
            return "unknown";
        }

        private static void CacheLicenseLocally(string licenseCode)
        {
            try
            {
                System.IO.Directory.CreateDirectory("license");
                // Simplistic local cache so a brief network hiccup on later
                // launches doesn't lock the user out. This gets replaced
                // with a properly signed token once the backend is defined.
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine("license", "cache.dat"),
                    licenseCode.Trim());
            }
            catch { /* non-fatal */ }
        }
    }
}
