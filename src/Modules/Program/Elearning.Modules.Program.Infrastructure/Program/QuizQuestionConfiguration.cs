using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
  public void Configure(EntityTypeBuilder<QuizQuestion> builder)
  {
    builder.ToTable("table_quiz_questions", "programs");
    builder.HasKey(q => q.question_id);

    builder.Property(q => q.question_id).HasDefaultValueSql("gen_random_uuid()");
    builder.Property(q => q.quiz_id).IsRequired();
    builder.Property(q => q.question_text).HasColumnType("text").IsRequired();
    builder.Property(q => q.question_type).HasConversion<string>().IsRequired();
    builder.Property(q => q.points).HasPrecision(5, 2).HasDefaultValue(1.0m).IsRequired();
    builder.Property(q => q.question_order).IsRequired();
    builder.Property(q => q.explanation).HasColumnType("text");
    builder.Property(q => q.is_required).HasDefaultValue(true).IsRequired();
    builder.Property(q => q.randomize_answers).HasDefaultValue(false).IsRequired();
    builder.Property(q => q.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(q => q.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(q => q.created_by).IsRequired();

    // Indexes
    builder.HasIndex(q => q.quiz_id);
    builder.HasIndex(q => new { q.quiz_id, q.question_order }).IsUnique();
    builder.HasIndex(q => q.question_type);

    // Foreign key relationships
    builder.HasOne<Quiz>()
        .WithMany()
        .HasForeignKey(q => q.quiz_id)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
