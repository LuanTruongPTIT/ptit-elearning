using Elearning.Modules.Users.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Users.Infrastructure.Users;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
  public void Configure(EntityTypeBuilder<Role> builder)
  {
    builder.ToTable("table_roles");
    builder.HasKey(r => r.name);
    builder.Property(r => r.name).HasMaxLength(50);

    builder
        .HasMany<Elearning.Modules.Users.Domain.Users.User>()
        .WithMany(u => u.Roles)
        .UsingEntity<Dictionary<string, object>>(
            "table_user_roles", // Tên bảng trung gian
            j => j
                .HasOne<Elearning.Modules.Users.Domain.Users.User>()
                .WithMany()
                .HasForeignKey("user_id")
                .HasConstraintName("table_user_roles_user_id_fkey"),
            j => j
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey("role_name")
                .HasConstraintName("table_user_roles_role_name_fkey"),
            j =>
            {
              j.HasKey("user_id", "role_name")
                   .HasName("table_user_roles_pkey");

              j.ToTable("table_user_roles");
            });
    builder.HasData(
      Role.Administrator,
      Role.Lecturer,
      Role.Student
      );
  }
}