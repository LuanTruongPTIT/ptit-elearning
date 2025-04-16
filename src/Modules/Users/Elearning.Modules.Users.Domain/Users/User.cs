

using Elearning.Common.Domain;

namespace Elearning.Modules.Users.Domain.Users;

public sealed class User : Entity
{
  private readonly List<Role> _roles = [];
  public IReadOnlyCollection<Role> Roles => _roles.ToList();
  private User() { }
  public Guid id { get; private set; }
  public string? username { get; private set; }
  public string password_hash { get; private set; }
  public string email { get; private set; }
  public string phone_number { get; private set; }
  public string full_name { get; private set; }
  public DateTime? date_of_birth { get; private set; }
  public Gender gender { get; private set; }
  public string? avatar_url { get; private set; }
  public string? address { get; private set; }
  public AccountUserStatus account_status { get; private set; }
  public DateTime created_at { get; private set; }
  public DateTime updated_at { get; private set; }

  public static User Create(string email,
                            string fullName,
                            string password,
                            Role role,
                            string phoneNumber,
                            string username,
                            string address,
                            string avatarUrl,
                            DateTime dateOfBirth,
                            Gender gender
                            )
  {
    var user = new User
    {
      id = Guid.NewGuid(),
      email = email,
      full_name = fullName,
      password_hash = password,
      phone_number = phoneNumber,
      username = username,
      address = address,
      avatar_url = avatarUrl,
      date_of_birth = dateOfBirth,
      gender = gender,
      account_status = AccountUserStatus.Inactive,
      created_at = DateTime.UtcNow,
      updated_at = DateTime.UtcNow
    };
    user._roles.Add(role);
    return user;
  }
}
