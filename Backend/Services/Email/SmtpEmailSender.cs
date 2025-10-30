using Backend.Settings;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Backend.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendSurveyInvitationAsync(
            string recipientEmail,
            string surveyTitle,
            string responseLink,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_settings.OverrideDropDirectory))
            {
                await WriteEmailToDropDirectory(recipientEmail, surveyTitle, responseLink, cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
                string.IsNullOrWhiteSpace(_settings.FromAddress))
            {
                _logger.LogWarning("SMTP settings missing; skipping email send for {Recipient}", recipientEmail);
                return;
            }

            using var client = new SmtpClient(_settings.SmtpHost!, _settings.SmtpPort ?? 587)
            {
                EnableSsl = _settings.UseSsl
            };

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
            }

            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress!, _settings.FromName),
                Subject = $"You're invited: {surveyTitle}",
                Body = BuildBody(surveyTitle, responseLink),
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(recipientEmail));

            _logger.LogInformation("Sending survey invitation to {Recipient}", recipientEmail);
            await client.SendMailAsync(message, cancellationToken);
        }

        private async Task WriteEmailToDropDirectory(
            string recipientEmail,
            string surveyTitle,
            string responseLink,
            CancellationToken cancellationToken)
        {
            var directory = _settings.OverrideDropDirectory!;
            Directory.CreateDirectory(directory);

            var filename = Path.Combine(
                directory,
                $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.txt");

            var contents = new StringBuilder()
                .AppendLine($"To: {recipientEmail}")
                .AppendLine($"Subject: You're invited: {surveyTitle}")
                .AppendLine()
                .AppendLine(BuildBody(surveyTitle, responseLink))
                .ToString();

            await File.WriteAllTextAsync(filename, contents, cancellationToken);
            _logger.LogInformation("Wrote invitation email to {File}", filename);
        }

        private static string BuildBody(string surveyTitle, string responseLink)
        {
            return $@"Hello,

You have been invited to take the survey ""{surveyTitle}"".

Your responses are completely anonymous, and this link can only be used once.

Start the survey: {responseLink}

If you were not expecting this email, you can safely ignore it.

Thank you!";
        }
    }
}
