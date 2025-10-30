using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts.Requests
{
    public class SubmitSurveyResponseRequest
    {
        [Required]
        public string Token { get; set; } = default!;

        [Required]
        [MinLength(1, ErrorMessage = "You must answer each survey question.")]
        public List<SubmitSurveyAnswerRequest> Answers { get; set; } = new();
    }

    public class SubmitSurveyAnswerRequest
    {
        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Answer { get; set; } = default!;
    }
}
