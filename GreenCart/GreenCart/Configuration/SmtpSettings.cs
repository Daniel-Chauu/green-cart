namespace GreenCart.Configuration
{
    /// <summary>
    /// SMTP configuration for sending emails via MailKit.
    /// For Gmail, you must use an "App Password" (not your regular Gmail password).
    /// Generate one at: https://myaccount.google.com/apppasswords
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
    }
}
