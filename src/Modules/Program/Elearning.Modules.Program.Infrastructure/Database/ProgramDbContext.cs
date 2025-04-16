using Microsoft.EntityFrameworkCore;
using Elearning.Modules.Program.Domain.Program;
using Elearning.Modules.Program.Infrastructure.Program;
using Elearning.Modules.Program.Application.Data;

namespace Elearning.Modules.Program.Infrastructure.Database;

public sealed class ProgramDbContext : DbContext, IProgramUnitOfWork
{
  public ProgramDbContext(DbContextOptions<ProgramDbContext> options)
      : base(options)
  {
  }

  internal DbSet<Courses> Courses { get; set; }
  internal DbSet<ProgramUnit> ProgramUnits { get; set; }
  internal DbSet<Department> Departments { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Schemas.Program);

    // Apply configurations
    modelBuilder.ApplyConfiguration(new CourseConfiguration());
    modelBuilder.ApplyConfiguration(new ProgramUnitConfiguration());
    modelBuilder.ApplyConfiguration(new DepartmentConfiguration());

    // Nếu có dùng Inbox/Outbox cho module này thì thêm như sau:
    // modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    // modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
  }
}
