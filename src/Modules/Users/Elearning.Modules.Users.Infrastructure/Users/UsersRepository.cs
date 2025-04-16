using Elearning.Modules.Users.Domain.Users;
using Elearning.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Elearning.Modules.Users.Infrastructure.Users;

internal sealed class UsersRepository(UsersDbContext dbContext) : IUserRepository
{
  public void Add(Domain.Users.User user)
  {
    foreach (Role role in user.Roles)
    {
      dbContext.Attach(role);
    }

    dbContext.Users.Add(user);
  }

  public async Task<Domain.Users.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await dbContext.Users.Include(x => x.Roles)
                                .SingleOrDefaultAsync(x => x.email == email, cancellationToken);
  }
}
