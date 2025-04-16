using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Common.Infrastructure.Inbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
  public void Configure(EntityTypeBuilder<InboxMessage> builder)
  {
    builder.ToTable("inbox_messages");
    builder.HasKey(e => e.Id);
    builder.Property(e => e.Content).HasMaxLength(2000).HasColumnType("jsonb");
  }
}