using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
  public void Configure(EntityTypeBuilder<QuizAttempt> builder)
  {
    builder.ToTable("table_quiz_attempts", "programs");
    builder.HasKey(a => a.attempt_id);

    builder.Property(a => a.attempt_id).HasDefaultValueSql("gen_random_uuid()");
    builder.Property(a => a.quiz_id).IsRequired();
    builder.Property(a => a.student_id).IsRequired();
    builder.Property(a => a.attempt_number).HasDefaultValue(1).IsRequired();
    builder.Property(a => a.status).HasConversion<string>().IsRequired();
    builder.Property(a => a.started_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(a => a.submitted_at);
    builder.Property(a => a.time_taken_seconds);
    builder.Property(a => a.total_score).HasPrecision(8, 2).HasDefaultValue(0).IsRequired();
    builder.Property(a => a.max_possible_score).HasPrecision(8, 2).HasDefaultValue(0).IsRequired();
    builder.Property(a => a.percentage_score).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
    builder.Property(a => a.passed);
    builder.Property(a => a.ip_address).HasMaxLength(45);
    builder.Property(a => a.user_agent).HasColumnType("text");
    builder.Property(a => a.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(a => a.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(a => a.created_by).IsRequired();

    // Indexes
    builder.HasIndex(a => a.quiz_id);
    builder.HasIndex(a => a.student_id);
    builder.HasIndex(a => a.status);
    builder.HasIndex(a => a.submitted_at);
    builder.HasIndex(a => new { a.quiz_id, a.student_id, a.attempt_number }).IsUnique();

    // Foreign key relationships
    builder.HasOne<Quiz>()
        .WithMany()
        .HasForeignKey(a => a.quiz_id)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
