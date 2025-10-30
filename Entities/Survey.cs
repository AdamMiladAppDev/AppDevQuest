using System.Text.Json.Serialization;

namespace Backend.Entities
{
    public class Survey
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<SurveyQuestion> Questions { get; set; } = Array.Empty<SurveyQuestion>();
    }

    public class SurveyQuestion
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public string Prompt { get; set; } = default!;
        public string QuestionType { get; set; } = "text";
        public string? OptionsJson { get; set; }
        public int Order { get; set; }
    }

    public class SurveyInvitation
    {
        public string TokenHash { get; set; } = default!;
        public Guid SurveyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public class SurveyResponse
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string InvitationTokenHash { get; set; } = default!;
        public IReadOnlyCollection<SurveyAnswer> Answers { get; set; } = Array.Empty<SurveyAnswer>();
    }

    public class SurveyAnswer
    {
        public Guid Id { get; set; }
        public Guid ResponseId { get; set; }
        public Guid QuestionId { get; set; }
        public string AnswerText { get; set; } = default!;
    }
}
