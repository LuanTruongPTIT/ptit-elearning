using Elearning.Modules.Users.Application.Data;
using Microsoft.EntityFrameworkCore;
using Elearning.Modules.Users.Domain.Users;
using Elearning.Common.Infrastructure.Outbox;
using Elearning.Common.Infrastructure.Inbox;
using Elearning.Modules.Users.Infrastructure.Users;

namespace Elearning.Modules.Users.Infrastructure.Database;

public sealed class UsersDbContext : DbContext, IUnitOfWork
{
  public UsersDbContext(DbContextOptions<UsersDbContext> options)
      : base(options)
  {
  }

  internal DbSet<Elearning.Modules.Users.Domain.Users.User> Users { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Schemas.Users);
    modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
    modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new RoleConfiguration());
  }
}
