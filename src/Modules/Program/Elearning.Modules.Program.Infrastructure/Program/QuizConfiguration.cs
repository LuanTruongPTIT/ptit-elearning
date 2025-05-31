using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
  public void Configure(EntityTypeBuilder<Quiz> builder)
  {
    builder.ToTable("table_quizzes", "programs");
    builder.HasKey(q => q.quiz_id);

    builder.Property(q => q.quiz_id).HasDefaultValueSql("gen_random_uuid()");
    builder.Property(q => q.assignment_id).IsRequired();
    builder.Property(q => q.quiz_title).HasMaxLength(500).IsRequired();
    builder.Property(q => q.quiz_description).HasColumnType("text");
    builder.Property(q => q.time_limit_minutes);
    builder.Property(q => q.max_attempts).HasDefaultValue(1).IsRequired();
    builder.Property(q => q.shuffle_questions).HasDefaultValue(false).IsRequired();
    builder.Property(q => q.shuffle_answers).HasDefaultValue(false).IsRequired();
    builder.Property(q => q.show_results_immediately).HasDefaultValue(true).IsRequired();
    builder.Property(q => q.show_correct_answers).HasDefaultValue(true).IsRequired();
    builder.Property(q => q.passing_score_percentage).HasPrecision(5, 2);
    builder.Property(q => q.allow_review).HasDefaultValue(true).IsRequired();
    builder.Property(q => q.auto_submit_on_timeout).HasDefaultValue(true).IsRequired();
    builder.Property(q => q.total_points).HasPrecision(8, 2).HasDefaultValue(0).IsRequired();
    builder.Property(q => q.total_questions).HasDefaultValue(0).IsRequired();
    builder.Property(q => q.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(q => q.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(q => q.created_by).IsRequired();

    // Indexes
    builder.HasIndex(q => q.assignment_id);
    builder.HasIndex(q => q.created_by);
    builder.HasIndex(q => q.created_at);
  }
}
