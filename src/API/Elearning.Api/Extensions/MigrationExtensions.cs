using Elearning.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
namespace Elearning.Api.Extensions;

public static class MigrationExtensions
{
  public static void ApplyMigrations(this IApplicationBuilder app)
  {
    using IServiceScope scope = app.ApplicationServices.CreateScope();

    ApplyMigration<UsersDbContext>(scope);
    // ApplyMigration<EventsDbContext>(scope);
    // ApplyMigration<TicketingDbContext>(scope);
    // ApplyMigration<AttendanceDbContext>(scope);
  }

  private static void ApplyMigration<TDbContext>(IServiceScope scope)
      where TDbContext : DbContext
  {
    using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

    context.Database.Migrate();
  }
}
