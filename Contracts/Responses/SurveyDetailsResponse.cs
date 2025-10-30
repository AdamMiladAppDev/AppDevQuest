namespace Backend.Contracts.Responses
{
    public class SurveyDetailsResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int InvitationCount { get; set; }
        public int ResponseCount { get; set; }
        public IReadOnlyCollection<SurveyQuestionResponse> Questions { get; set; } = Array.Empty<SurveyQuestionResponse>();
    }

    public class SurveyQuestionResponse
    {
        public Guid Id { get; set; }
        public string Prompt { get; set; } = default!;
        public string QuestionType { get; set; } = "text";
        public IReadOnlyCollection<string> Options { get; set; } = Array.Empty<string>();
    }
}
