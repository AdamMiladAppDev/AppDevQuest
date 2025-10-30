using Backend.Entities;
using Backend.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services.Security
{
    public class JwtTokenService
    {
        private readonly AuthSettings _settings;

        public JwtTokenService(IOptions<AuthSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string token, DateTime expiresAt) CreateToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var rawKey = Encoding.UTF8.GetBytes(_settings.JwtSecret);

            if (rawKey.Length < 32)
            {
                rawKey = SHA256.HashData(rawKey);
            }

            var securityKey = new SymmetricSecurityKey(rawKey);

            var expires = DateTime.UtcNow.AddMinutes(_settings.TokenLifetimeMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email)
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = handler.CreateToken(descriptor);
            var token = handler.WriteToken(securityToken);
            return (token, expires);
        }
    }
}
