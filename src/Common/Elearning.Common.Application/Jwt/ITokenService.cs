using Elearning.Common.Domain;

namespace Elearning.Common.Application.Jwt;

public interface ITokenService
{
  Task<TokenModel> GenerateTokens(string userId, IEnumerable<string> roles);
  Task<TokenModel> RefreshToken(string accessToken, string refreshToken);
  Task RevokeToken(string userId);
  Task<bool> ValidateToken(string token);
}