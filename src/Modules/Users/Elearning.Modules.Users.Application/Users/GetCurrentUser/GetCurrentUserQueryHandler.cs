using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Users.Domain.Users;


namespace Elearning.Modules.Users.Application.Users.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IUserRepository userRepository) : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
  public async Task<Result<GetCurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
  {
    // Get the current user ID from the claims


    var user = await userRepository.GetByIdAsync(request.user_id, cancellationToken);

    if (user == null)
    {
      return Result.Failure<GetCurrentUserResponse>(Error.NotFound("GetCurrentUser", "User not found"));
    }

    // Map the user entity to the response
    var response = new GetCurrentUserResponse(
        id: user.id.ToString(),
        username: user.username ?? string.Empty,
        email: user.email,
        full_name: user.full_name,
        phone_number: user.phone_number,
        address: user.address,
        avatar_url: user.avatar_url,
        date_of_birth: user.date_of_birth,
        gender: user.gender.ToString(),
        account_status: user.account_status.ToString(),
        role: user.Roles.FirstOrDefault()?.name ?? "Unknown",
        created_at: user.created_at
    );

    return Result.Success(response);
  }
}
