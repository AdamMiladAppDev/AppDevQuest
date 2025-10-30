namespace Backend.Services.Email
{
    public interface IEmailSender
    {
        Task SendSurveyInvitationAsync(
            string recipientEmail,
            string surveyTitle,
            string responseLink,
            CancellationToken cancellationToken);
    }
}
