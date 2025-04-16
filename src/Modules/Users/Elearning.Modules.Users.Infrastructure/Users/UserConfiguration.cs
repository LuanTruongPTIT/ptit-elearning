using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Users.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<Elearning.Modules.Users.Domain.Users.User>
{
  public void Configure(EntityTypeBuilder<Elearning.Modules.Users.Domain.Users.User> builder)
  {
    builder.ToTable("table_users", "users");
    builder.HasKey(u => u.id);
    builder.HasIndex(u => u.email).IsUnique();
    builder.HasIndex(u => u.username);
    builder.Property(u => u.password_hash).HasMaxLength(255);
    builder.Property(u => u.full_name).HasMaxLength(255);
    builder.Property(u => u.phone_number).HasMaxLength(255);
    builder.Property(u => u.address).HasMaxLength(255);
    builder.Property(u => u.avatar_url).HasMaxLength(255);
    builder.Property(u => u.date_of_birth);
    builder.Property(u => u.gender);
    builder.Property(u => u.account_status);
    builder.Property(u => u.created_at);
    builder.Property(u => u.updated_at);
  }
}
