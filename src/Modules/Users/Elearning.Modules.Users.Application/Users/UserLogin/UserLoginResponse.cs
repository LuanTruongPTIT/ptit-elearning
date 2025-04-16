namespace Elearning.Modules.Users.Application.Users.UserLogin;

public sealed record UserLoginResponse(string role, string access_token, int expires_in_access_token, string refresh_token, int expires_in_refresh_tokens);

