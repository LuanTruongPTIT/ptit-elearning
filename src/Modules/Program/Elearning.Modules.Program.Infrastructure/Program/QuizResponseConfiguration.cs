using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizResponseConfiguration : IEntityTypeConfiguration<QuizResponse>
{
  public void Configure(EntityTypeBuilder<QuizResponse> builder)
  {
    builder.ToTable("table_quiz_responses", "programs");
    builder.HasKey(r => r.response_id);

    builder.Property(r => r.response_id).HasDefaultValueSql("gen_random_uuid()");
    builder.Property(r => r.attempt_id).IsRequired();
    builder.Property(r => r.question_id).IsRequired();
    builder.Property(r => r.selected_answer_ids).HasColumnType("json");
    builder.Property(r => r.text_response).HasColumnType("text");
    builder.Property(r => r.is_correct).HasDefaultValue(false).IsRequired();
    builder.Property(r => r.points_earned).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
    builder.Property(r => r.points_possible).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
    builder.Property(r => r.time_spent_seconds);
    builder.Property(r => r.answered_at);
    builder.Property(r => r.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    builder.Property(r => r.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

    // Indexes
    builder.HasIndex(r => r.attempt_id);
    builder.HasIndex(r => r.question_id);
    builder.HasIndex(r => r.is_correct);
    builder.HasIndex(r => r.answered_at);
    builder.HasIndex(r => new { r.attempt_id, r.question_id }).IsUnique();

    // Foreign key relationships
    builder.HasOne<QuizAttempt>()
        .WithMany()
        .HasForeignKey(r => r.attempt_id)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne<QuizQuestion>()
        .WithMany()
        .HasForeignKey(r => r.question_id)
        .OnDelete(DeleteBehavior.Restrict);
  }
}
