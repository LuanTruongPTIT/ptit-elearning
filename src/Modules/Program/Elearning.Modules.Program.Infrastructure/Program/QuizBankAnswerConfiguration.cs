using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizBankAnswerConfiguration : IEntityTypeConfiguration<QuizBankAnswer>
{
    public void Configure(EntityTypeBuilder<QuizBankAnswer> builder)
    {
        builder.ToTable("table_quiz_bank_answers", "programs");
        builder.HasKey(a => a.bank_answer_id);

        builder.Property(a => a.bank_answer_id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.bank_question_id).IsRequired();
        builder.Property(a => a.answer_text).HasColumnType("text").IsRequired();
        builder.Property(a => a.is_correct).HasDefaultValue(false).IsRequired();
        builder.Property(a => a.answer_order).IsRequired();
        builder.Property(a => a.answer_explanation).HasColumnType("text");
        builder.Property(a => a.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(a => a.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(a => a.created_by).IsRequired();

        // Indexes
        builder.HasIndex(a => a.bank_question_id);
        builder.HasIndex(a => new { a.bank_question_id, a.answer_order }).IsUnique();
        builder.HasIndex(a => new { a.bank_question_id, a.is_correct });

        // Foreign key relationships
        builder.HasOne<QuizBankQuestion>()
            .WithMany()
            .HasForeignKey(a => a.bank_question_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
