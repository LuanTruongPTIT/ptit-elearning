using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizQuestionBankConfiguration : IEntityTypeConfiguration<QuizQuestionBank>
{
    public void Configure(EntityTypeBuilder<QuizQuestionBank> builder)
    {
        builder.ToTable("table_quiz_question_banks", "programs");
        builder.HasKey(b => b.bank_id);

        builder.Property(b => b.bank_id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.bank_name).HasMaxLength(500).IsRequired();
        builder.Property(b => b.bank_description).HasColumnType("text");
        builder.Property(b => b.subject_area).HasMaxLength(200);
        builder.Property(b => b.difficulty_level).HasConversion<string>();
        builder.Property(b => b.is_public).HasDefaultValue(false).IsRequired();
        builder.Property(b => b.is_system_bank).HasDefaultValue(false).IsRequired();
        builder.Property(b => b.question_count).HasDefaultValue(0).IsRequired();
        builder.Property(b => b.usage_count).HasDefaultValue(0).IsRequired();
        builder.Property(b => b.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(b => b.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(b => b.created_by).IsRequired();

        // Indexes
        builder.HasIndex(b => b.subject_area);
        builder.HasIndex(b => b.difficulty_level);
        builder.HasIndex(b => b.is_public);
        builder.HasIndex(b => b.created_by);
    }
}
