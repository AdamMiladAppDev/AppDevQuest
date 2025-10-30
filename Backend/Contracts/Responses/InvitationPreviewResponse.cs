namespace Backend.Contracts.Responses
{
    public class InvitationPreviewResponse
    {
        public required string Email { get; init; }
        public required string Link { get; init; }
    }
}
