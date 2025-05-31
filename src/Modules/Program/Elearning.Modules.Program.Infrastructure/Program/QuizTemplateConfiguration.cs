using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizTemplateConfiguration : IEntityTypeConfiguration<QuizTemplate>
{
    public void Configure(EntityTypeBuilder<QuizTemplate> builder)
    {
        builder.ToTable("table_quiz_templates", "programs");
        builder.HasKey(t => t.template_id);

        builder.Property(t => t.template_id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.template_name).HasMaxLength(500).IsRequired();
        builder.Property(t => t.template_description).HasColumnType("text");
        builder.Property(t => t.category).HasMaxLength(100);
        builder.Property(t => t.template_data).HasColumnType("json").IsRequired();
        builder.Property(t => t.is_public).HasDefaultValue(false).IsRequired();
        builder.Property(t => t.is_system_template).HasDefaultValue(false).IsRequired();
        builder.Property(t => t.usage_count).HasDefaultValue(0).IsRequired();
        builder.Property(t => t.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(t => t.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(t => t.created_by).IsRequired();

        // Indexes
        builder.HasIndex(t => t.category);
        builder.HasIndex(t => t.is_public);
        builder.HasIndex(t => t.created_by);
        builder.HasIndex(t => t.usage_count);
        builder.HasIndex(t => t.is_system_template);
    }
}
