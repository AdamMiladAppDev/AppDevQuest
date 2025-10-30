namespace Backend.Contracts.Responses
{
    public class SurveyListItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int QuestionCount { get; set; }
        public int InvitationCount { get; set; }
        public int ResponseCount { get; set; }
    }
}
