using FluentValidation;

namespace Elearning.Modules.Users.Application.Users.UserLogin;

internal sealed class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
  public UserLoginCommandValidator()
  {
    RuleFor(x => x.email).NotEmpty().EmailAddress();
    RuleFor(x => x.password).NotEmpty().MinimumLength(8);
  }
}