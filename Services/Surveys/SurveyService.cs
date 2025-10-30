using Backend.Contracts.Requests;
using Backend.Contracts.Responses;
using Backend.Entities;
using Backend.Repository;
using Backend.Services.Email;
using Backend.Services.Security;
using Backend.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Linq;

namespace Backend.Services.Surveys
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _repository;
        private readonly IEmailSender _emailSender;
        private readonly TokenService _tokenService;
        private readonly ApplicationSettings _appSettings;
        private readonly ILogger<SurveyService> _logger;

        public SurveyService(
            ISurveyRepository repository,
            IEmailSender emailSender,
            TokenService tokenService,
            IOptions<ApplicationSettings> appSettings,
            ILogger<SurveyService> logger)
        {
            _repository = repository;
            _emailSender = emailSender;
            _tokenService = tokenService;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<SurveyDetailsResponse> CreateSurveyAsync(
            CreateSurveyRequest request,
            CancellationToken cancellationToken)
        {
            var survey = new Survey
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var questions = request.Questions
                                   .Select((q, index) => new SurveyQuestion
                                   {
                                       Id = Guid.NewGuid(),
                                       SurveyId = survey.Id,
                                       Prompt = q.Prompt.Trim(),
                                       QuestionType = "text",
                                       OptionsJson = null,
                                       Order = index
                                   })
                                   .ToList();

            await _repository.CreateSurveyAsync(survey, questions, cancellationToken);

            survey.Questions = questions;

            return MapToDetailsResponse(survey, invitations: 0, responses: 0);
        }

        public async Task<IReadOnlyCollection<SurveyListItemResponse>> GetSurveysAsync(CancellationToken cancellationToken)
        {
            var surveys = await _repository.GetAllSurveysAsync(cancellationToken);
            var list = new List<SurveyListItemResponse>();

            foreach (var survey in surveys)
            {
                var stats = await _repository.GetSurveyStatsAsync(survey.Id, cancellationToken);
                list.Add(new SurveyListItemResponse
                {
                    Id = survey.Id,
                    Title = survey.Title,
                    Description = survey.Description,
                    CreatedAt = survey.CreatedAt,
                    QuestionCount = survey.Questions.Count,
                    InvitationCount = stats.invitations,
                    ResponseCount = stats.responses
                });
            }

            return list
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public async Task<SurveyDetailsResponse?> GetSurveyAsync(Guid surveyId, CancellationToken cancellationToken)
        {
            var survey = await _repository.GetSurveyAsync(surveyId, cancellationToken);
            if (survey is null)
            {
                return null;
            }

            var stats = await _repository.GetSurveyStatsAsync(survey.Id, cancellationToken);
            return MapToDetailsResponse(survey, stats.invitations, stats.responses);
        }

        public async Task SendInvitationsAsync(Guid surveyId, SendInvitationsRequest request, CancellationToken cancellationToken)
        {
            var survey = await _repository.GetSurveyAsync(surveyId, cancellationToken);
            if (survey is null)
            {
                throw new InvalidOperationException("Survey not found.");
            }

            var normalizedEmails = request.Emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!normalizedEmails.Any())
            {
                throw new InvalidOperationException("No valid email addresses provided.");
            }

            foreach (var email in normalizedEmails)
            {
                var token = _tokenService.GenerateToken();
                var tokenHash = _tokenService.HashToken(token);

                var invitation = new SurveyInvitation
                {
                    TokenHash = tokenHash,
                    SurveyId = surveyId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = request.ExpiresAt
                };

                await _repository.AddInvitationAsync(invitation, cancellationToken);

                var link = BuildResponseLink(token);
                await _emailSender.SendSurveyInvitationAsync(email, survey.Title, link, cancellationToken);
            }
        }

        public async Task<SurveyForResponseDto?> GetSurveyForTokenAsync(string token, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenService.HashToken(token);
            var invitation = await _repository.GetInvitationByHashAsync(tokenHash, cancellationToken);

            if (invitation is null ||
                invitation.RespondedAt.HasValue ||
                (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value < DateTime.UtcNow))
            {
                return null;
            }

            var survey = await _repository.GetSurveyByInvitationHashAsync(tokenHash, cancellationToken);
            if (survey is null)
            {
                return null;
            }

            return new SurveyForResponseDto
            {
                SurveyId = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                ExpiresAt = invitation.ExpiresAt,
                Questions = survey.Questions
                    .OrderBy(q => q.Order)
                    .Select(ToQuestionResponse)
                    .ToList()
            };
        }

        public async Task SubmitSurveyResponseAsync(SubmitSurveyResponseRequest request, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenService.HashToken(request.Token);
            var invitation = await _repository.GetInvitationByHashAsync(tokenHash, cancellationToken);

            if (invitation is null)
            {
                throw new InvalidOperationException("Invalid or unknown token.");
            }

            if (invitation.RespondedAt.HasValue)
            {
                throw new InvalidOperationException("This invitation has already been used.");
            }

            if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("This invitation has expired.");
            }

            var survey = await _repository.GetSurveyByInvitationHashAsync(tokenHash, cancellationToken);
            if (survey is null)
            {
                throw new InvalidOperationException("Survey for invitation not found.");
            }

            var surveyQuestionIds = survey.Questions.Select(q => q.Id).ToHashSet();
            var providedQuestionIds = request.Answers.Select(a => a.QuestionId).ToList();

            if (providedQuestionIds.Count != surveyQuestionIds.Count)
            {
                throw new InvalidOperationException("You must answer all questions.");
            }

            if (!providedQuestionIds.All(id => surveyQuestionIds.Contains(id)))
            {
                throw new InvalidOperationException("Submitted answers do not match survey questions.");
            }

            var responseId = Guid.NewGuid();
            var answers = request.Answers.Select(a => new SurveyAnswer
            {
                Id = Guid.NewGuid(),
                ResponseId = responseId,
                QuestionId = a.QuestionId,
                AnswerText = a.Answer.Trim()
            }).ToList();

            var response = new SurveyResponse
            {
                Id = responseId,
                SurveyId = survey.Id,
                SubmittedAt = DateTime.UtcNow,
                InvitationTokenHash = tokenHash,
                Answers = answers
            };

            await _repository.SaveResponseAsync(response, cancellationToken);
            await _repository.MarkInvitationRespondedAsync(tokenHash, DateTime.UtcNow, cancellationToken);
        }

        private string BuildResponseLink(string token)
        {
            var baseUrl = _appSettings.ResponseBaseUrl.TrimEnd('/');
            return $"{baseUrl}/{token}";
        }

        private static SurveyDetailsResponse MapToDetailsResponse(Survey survey, int invitations, int responses)
        {
            return new SurveyDetailsResponse
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = survey.Description,
                CreatedAt = survey.CreatedAt,
                InvitationCount = invitations,
                ResponseCount = responses,
                Questions = survey.Questions
                    .OrderBy(q => q.Order)
                    .Select(ToQuestionResponse)
                    .ToList()
            };
        }

        private static SurveyQuestionResponse ToQuestionResponse(SurveyQuestion question)
        {
            var options = new List<string>();

            if (!string.IsNullOrWhiteSpace(question.OptionsJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<string>>(question.OptionsJson);
                    if (parsed is not null)
                    {
                        options = parsed;
                    }
                }
                catch
                {
                    // swallow parse errors and treat as no options.
                }
            }

            return new SurveyQuestionResponse
            {
                Id = question.Id,
                Prompt = question.Prompt,
                QuestionType = question.QuestionType,
                Options = options
            };
        }
    }
}
