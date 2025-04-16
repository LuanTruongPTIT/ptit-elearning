using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Common.Infrastructure.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    builder.ToTable("table_outbox_messages");

    builder.HasKey(o => o.Id);

    builder.Property(o => o.Content).HasMaxLength(2000).HasColumnType("jsonb");
  }
}
