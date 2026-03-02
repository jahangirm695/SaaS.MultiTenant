using SaaS.Domain.Entities;

namespace SaaS.Application.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    Task<User?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
