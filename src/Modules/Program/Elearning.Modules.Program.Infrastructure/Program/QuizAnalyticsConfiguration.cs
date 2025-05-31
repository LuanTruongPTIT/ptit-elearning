using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class QuizAnalyticsConfiguration : IEntityTypeConfiguration<QuizAnalytics>
{
    public void Configure(EntityTypeBuilder<QuizAnalytics> builder)
    {
        builder.ToTable("table_quiz_analytics", "programs");
        builder.HasKey(a => a.analytics_id);

        builder.Property(a => a.analytics_id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.quiz_id).IsRequired();
        builder.Property(a => a.period_type).HasConversion<string>().IsRequired();
        builder.Property(a => a.period_date).IsRequired();
        builder.Property(a => a.total_attempts).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.completed_attempts).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.in_progress_attempts).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.average_score).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.highest_score).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.lowest_score).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.median_score).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.average_time_seconds).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.fastest_time_seconds).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.slowest_time_seconds).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.passed_count).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.failed_count).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.pass_rate_percentage).HasPrecision(5, 2).HasDefaultValue(0).IsRequired();
        builder.Property(a => a.question_statistics).HasColumnType("json");
        builder.Property(a => a.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(a => a.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(a => a.created_by).IsRequired();

        // Indexes
        builder.HasIndex(a => a.quiz_id);
        builder.HasIndex(a => new { a.period_type, a.period_date });
        builder.HasIndex(a => a.period_date);

        // Constraints
        builder.HasIndex(a => new { a.quiz_id, a.period_type, a.period_date }).IsUnique();

        // Foreign key relationships
        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(a => a.quiz_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
