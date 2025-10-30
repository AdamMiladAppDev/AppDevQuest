namespace Backend.Settings
{
    public class EmailSettings
    {
        public string FromAddress { get; set; } = default!;
        public string FromName { get; set; } = "Survey Bot";
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public bool UseSsl { get; set; } = true;
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? OverrideDropDirectory { get; set; }
    }
}
