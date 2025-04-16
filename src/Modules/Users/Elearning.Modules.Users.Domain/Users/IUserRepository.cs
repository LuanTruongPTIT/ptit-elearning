namespace Elearning.Modules.Users.Domain.Users;

public interface IUserRepository
{
  Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

  void Add(User user);
}