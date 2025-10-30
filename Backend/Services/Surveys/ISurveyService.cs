using Backend.Contracts.Requests;
using Backend.Contracts.Responses;

namespace Backend.Services.Surveys
{
    public interface ISurveyService
    {
        Task<SurveyDetailsResponse> CreateSurveyAsync(CreateSurveyRequest request, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<SurveyListItemResponse>> GetSurveysAsync(CancellationToken cancellationToken);
        Task<SurveyDetailsResponse?> GetSurveyAsync(Guid surveyId, CancellationToken cancellationToken);
        Task SendInvitationsAsync(Guid surveyId, SendInvitationsRequest request, CancellationToken cancellationToken);
        Task<SurveyForResponseDto?> GetSurveyForTokenAsync(string token, CancellationToken cancellationToken);
        Task SubmitSurveyResponseAsync(SubmitSurveyResponseRequest request, CancellationToken cancellationToken);
    }
}
