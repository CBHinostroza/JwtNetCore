namespace JwtNet6.Models.Settings
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public int DurationInInDays { get; set; }
        public string Subject { get; set; } = string.Empty;
    }
}
