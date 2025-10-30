using Backend.Contracts.Requests;
using Backend.Contracts.Responses;
using Backend.Repository;
using Backend.Services.Security;
using BCrypt.Net;

namespace Backend.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly JwtTokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository users, JwtTokenService tokenService, ILogger<AuthService> logger)
        {
            _users = users;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Login failed for {Email}: user not found", request.Email);
                return null;
            }

            var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!valid)
            {
                _logger.LogWarning("Login failed for {Email}: invalid password", request.Email);
                return null;
            }

            var (token, expires) = _tokenService.CreateToken(user);
            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expires
            };
        }
    }
}
