namespace Backend.Settings
{
    public class AuthSettings
    {
        public string JwtSecret { get; set; } = null!;
        public int TokenLifetimeMinutes { get; set; } = 120;
    }
}
