using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Users.UserLogin;

public sealed record UserLoginCommand(string email, string password) : ICommand<UserLoginResponse>;