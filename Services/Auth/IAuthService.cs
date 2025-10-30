using Backend.Contracts.Requests;
using Backend.Contracts.Responses;

namespace Backend.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
