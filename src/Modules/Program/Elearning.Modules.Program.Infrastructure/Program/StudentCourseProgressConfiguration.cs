using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class StudentCourseProgressConfiguration : IEntityTypeConfiguration<StudentCourseProgress>
{
    public void Configure(EntityTypeBuilder<StudentCourseProgress> builder)
    {
        builder.ToTable("table_student_course_progress");

        builder.HasKey(x => x.id);

        builder.Property(x => x.student_id)
            .IsRequired();

        builder.Property(x => x.teaching_assign_course_id)
            .IsRequired();

        builder.Property(x => x.total_lectures)
            .IsRequired();

        builder.Property(x => x.completed_lectures)
            .IsRequired();

        builder.Property(x => x.progress_percentage)
            .IsRequired();

        builder.Property(x => x.status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.last_accessed)
            .IsRequired();

        builder.Property(x => x.created_at)
            .IsRequired();

        builder.Property(x => x.updated_at)
            .IsRequired();

        // Add unique constraint
        builder.HasIndex(x => new { x.student_id, x.teaching_assign_course_id })
            .IsUnique();
    }
}
