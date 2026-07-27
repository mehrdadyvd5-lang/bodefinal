namespace BodyComposition.App.Models
{
    public class LicenseCheckResult
    {
        public bool IsValid { get; set; }
        public bool NetworkError { get; set; }
        public string Message { get; set; }
        public string ExpiresAtUtc { get; set; }
    }
}
