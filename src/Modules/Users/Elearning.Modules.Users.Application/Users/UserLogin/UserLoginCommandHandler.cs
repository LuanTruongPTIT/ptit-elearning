using Elearning.Common.Application.Jwt;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Users.Application.Data;
using Elearning.Modules.Users.Domain.Users;
using Serilog;

namespace Elearning.Modules.Users.Application.Users.UserLogin;

internal sealed class UserLoginCommanndHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService
) : ICommandHandler<UserLoginCommand, UserLoginResponse>
{
  public async Task<Result<UserLoginResponse>> Handle(UserLoginCommand request, CancellationToken cancellationToken)
  {
    var user = await userRepository.GetByEmailAsync(request.email);

    if (user is null)
    {
      return Result.Failure<UserLoginResponse>(Error.Validation("Email", "Email is not found!"));
    }
    Log.Information("User found: {User}", user);
    bool comparePassword = VerifyPassword(request.password, user.password_hash);

    if (!comparePassword)
    {
      return Result.Failure<UserLoginResponse>(Error.Validation("Password", "Password is not correct!"));
    }

    TokenModel token = await tokenService.GenerateTokens(user.id.ToString(), user.Roles.Select(x => x.name));

    var userLoginResponse = new UserLoginResponse(user.Roles.Select(x => x.name).FirstOrDefault(), token.access_token, token.expires_in_access_token, token.refresh_token, token.expires_in_refresh_token);
    return Result.Success(userLoginResponse);
  }
  public bool VerifyPassword(string password, string passwordHash)
  {
    try
    {
      return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
    catch
    {
      return false;
    }
  }
}
