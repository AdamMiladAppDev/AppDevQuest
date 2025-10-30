namespace Backend.Contracts.Responses
{
    public class SurveyForResponseDto
    {
        public Guid SurveyId { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public IReadOnlyCollection<SurveyQuestionResponse> Questions { get; set; } = Array.Empty<SurveyQuestionResponse>();
        public DateTime? ExpiresAt { get; set; }
    }
}
