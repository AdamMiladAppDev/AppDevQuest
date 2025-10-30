using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts.Requests
{
    public class SendInvitationsRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one recipient email is required.")]
        public List<string> Emails { get; set; } = new();

        public DateTime? ExpiresAt { get; set; }
    }
}
