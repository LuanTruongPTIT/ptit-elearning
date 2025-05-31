using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizBankQuestionConfiguration : IEntityTypeConfiguration<QuizBankQuestion>
{
    public void Configure(EntityTypeBuilder<QuizBankQuestion> builder)
    {
        builder.ToTable("table_quiz_bank_questions", "programs");
        builder.HasKey(q => q.bank_question_id);

        builder.Property(q => q.bank_question_id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(q => q.bank_id).IsRequired();
        builder.Property(q => q.question_text).HasColumnType("text").IsRequired();
        builder.Property(q => q.question_type).HasConversion<string>().IsRequired();
        builder.Property(q => q.default_points).HasPrecision(5, 2).HasDefaultValue(1.0m).IsRequired();
        builder.Property(q => q.explanation).HasColumnType("text");
        builder.Property(q => q.difficulty_level).HasConversion<string>();
        builder.Property(q => q.tags).HasColumnType("json");
        builder.Property(q => q.usage_count).HasDefaultValue(0).IsRequired();
        builder.Property(q => q.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(q => q.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(q => q.created_by).IsRequired();

        // Indexes
        builder.HasIndex(q => q.bank_id);
        builder.HasIndex(q => q.question_type);
        builder.HasIndex(q => q.difficulty_level);
        builder.HasIndex(q => q.usage_count);

        // Foreign key relationships
        builder.HasOne<QuizQuestionBank>()
            .WithMany()
            .HasForeignKey(q => q.bank_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
