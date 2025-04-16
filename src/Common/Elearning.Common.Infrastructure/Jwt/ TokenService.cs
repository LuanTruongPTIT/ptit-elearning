using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Elearning.Common.Application.Jwt;
using Elearning.Common.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Elearning.Common.Infrastructure.Jwt;

public class TokenService : ITokenService
{
  private readonly JwtSettings _jwtSettings;
  private readonly IConnectionMultiplexer _redis;

  public TokenService(IOptions<JwtSettings> jwtSettings, IConnectionMultiplexer redis)
  {
    _jwtSettings = jwtSettings.Value;
    _redis = redis;
  }

  public async Task<TokenModel> GenerateTokens(string userId, IEnumerable<string> roles)
  {
    var accessToken = GenerateAccessToken(userId, roles);
    var refreshToken = GenerateRefreshToken(userId, roles);

    // Lưu refresh token vào Redis
    var db = _redis.GetDatabase();
    await db.StringSetAsync(
        $"refresh_token:{userId}",
        refreshToken,
        TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays)
    );

    return new TokenModel
    {
      access_token = accessToken,
      refresh_token = refreshToken,
      expires_in_access_token = _jwtSettings.AccessTokenExpirationMinutes,
      expires_in_refresh_token = _jwtSettings.RefreshTokenExpirationDays
    };
  }

  private string GenerateAccessToken(string userId, IEnumerable<string> roles)
  {
    var claims = new List<Claim>
        {
            new Claim("sub", userId)
        };

    claims.AddRange(roles.Select(role => new Claim("role", role)));

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
        claims: claims,
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private string GenerateRefreshToken(string userId, IEnumerable<string> roles)
  {
    var claims = new List<Claim>
        {
            new Claim("sub", userId)
        };

    claims.AddRange(roles.Select(role => new Claim("role", role)));


    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationDays),
        claims: claims,
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public async Task<TokenModel> RefreshToken(string accessToken, string refreshToken)
  {
    var principal = GetPrincipalFromExpiredToken(accessToken);
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var db = _redis.GetDatabase();
    var storedRefreshToken = await db.StringGetAsync($"refresh_token:{userId}");

    if (!storedRefreshToken.HasValue || storedRefreshToken != refreshToken)
      throw new SecurityTokenException("Invalid refresh token");

    var username = principal.FindFirst(ClaimTypes.Name)?.Value;
    var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

    return await GenerateTokens(userId, roles);
  }

  public async Task RevokeToken(string userId)
  {
    var db = _redis.GetDatabase();
    await db.KeyDeleteAsync($"refresh_token:{userId}");
  }

  public Task<bool> ValidateToken(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    try
    {
      tokenHandler.ValidateToken(token, new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken validatedToken);

      return Task.FromResult(true);
    }
    catch
    {
      return Task.FromResult(false);
    }
  }

  private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
      ValidateIssuer = false,
      ValidateAudience = false,
      ValidateLifetime = false
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

    if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
      throw new SecurityTokenException("Invalid token");

    return principal;
  }
}