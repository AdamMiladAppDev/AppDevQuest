using Backend.Entities;

namespace Backend.Repository
{
    public interface ISurveyRepository
    {
        Task<Guid> CreateSurveyAsync(Survey survey, IEnumerable<SurveyQuestion> questions, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Survey>> GetAllSurveysAsync(CancellationToken cancellationToken);
        Task<Survey?> GetSurveyAsync(Guid surveyId, CancellationToken cancellationToken);
        Task<Survey?> GetSurveyByInvitationHashAsync(string tokenHash, CancellationToken cancellationToken);
        Task<(int invitations, int responses)> GetSurveyStatsAsync(Guid surveyId, CancellationToken cancellationToken);
        Task<int> GetInvitationCountAsync(Guid surveyId, CancellationToken cancellationToken);
        Task<int> GetResponseCountAsync(Guid surveyId, CancellationToken cancellationToken);
        Task AddInvitationAsync(SurveyInvitation invitation, CancellationToken cancellationToken);
        Task<SurveyInvitation?> GetInvitationByHashAsync(string tokenHash, CancellationToken cancellationToken);
        Task MarkInvitationRespondedAsync(string tokenHash, DateTime respondedAt, CancellationToken cancellationToken);
        Task<Guid> SaveResponseAsync(SurveyResponse response, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<SurveyInvitation>> GetInvitationsAsync(Guid surveyId, CancellationToken cancellationToken);
    }
}
