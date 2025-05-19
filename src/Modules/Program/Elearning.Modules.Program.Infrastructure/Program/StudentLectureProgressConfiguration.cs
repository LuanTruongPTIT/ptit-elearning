using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class StudentLectureProgressConfiguration : IEntityTypeConfiguration<StudentLectureProgress>
{
    public void Configure(EntityTypeBuilder<StudentLectureProgress> builder)
    {
        builder.ToTable("table_student_lecture_progresses", "enrollment");

        builder.HasKey(x => x.id);

        builder.Property(x => x.student_id)
            .IsRequired();

        builder.Property(x => x.lecture_id)
            .IsRequired();

        builder.Property(x => x.watch_position)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.progress_percentage)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.is_completed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.last_accessed);

        builder.Property(x => x.created_at)
            .IsRequired();

        builder.Property(x => x.updated_at)
            .IsRequired();

        // Add indexes
        builder.HasIndex(x => x.student_id);
        builder.HasIndex(x => x.lecture_id);
        builder.HasIndex(x => new { x.student_id, x.lecture_id }).IsUnique();
    }
}
