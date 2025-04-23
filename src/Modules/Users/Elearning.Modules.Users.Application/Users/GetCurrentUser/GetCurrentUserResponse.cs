namespace Elearning.Modules.Users.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    string id,
    string username,
    string email,
    string full_name,
    string? phone_number,
    string? address,
    string? avatar_url,
    DateTime? date_of_birth,
    string? gender,
    string account_status,
    string role,
    DateTime created_at);
