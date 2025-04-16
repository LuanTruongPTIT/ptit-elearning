using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Common.Infrastructure.Inbox;

public sealed class InboxMessageConsumerConfiguration : IEntityTypeConfiguration<InboxMessageConsumer>
{
  public void Configure(EntityTypeBuilder<InboxMessageConsumer> builder)
  {
    builder.ToTable("table_inbox_message_consumers");
    builder.HasKey(o => new { o.InboxMessageId, o.Name });
    builder.Property(o => o.Name).HasMaxLength(500);
  }
}
