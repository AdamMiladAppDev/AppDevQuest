using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts.Requests
{
    public class CreateSurveyRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MinLength(1, ErrorMessage = "Provide at least one question.")]
        public List<CreateSurveyQuestionRequest> Questions { get; set; } = new();
    }

    public class CreateSurveyQuestionRequest
    {
        [Required]
        [MaxLength(500)]
        public string Prompt { get; set; } = default!;
    }
}
