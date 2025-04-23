using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class LectureConfiguration : IEntityTypeConfiguration<Lecture>
{
  public void Configure(EntityTypeBuilder<Lecture> builder)
  {
    builder.ToTable("table_lectures");
    builder.HasKey(c => c.id);

    builder.Property(c => c.title).HasMaxLength(255).IsRequired();
    builder.Property(c => c.description).HasColumnType("text");
    builder.Property(c => c.content_type).HasMaxLength(50).IsRequired();
    builder.Property(c => c.content_url).IsRequired();
    builder.Property(c => c.youtube_video_id).HasMaxLength(50);
    builder.Property(c => c.duration);
    builder.Property(c => c.is_published).HasDefaultValue(false);
    builder.Property(c => c.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
    builder.Property(c => c.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

    // Foreign key relationships
    builder.HasOne<Courses>()
        .WithMany()
        .HasForeignKey(c => c.course_id)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne<TeachingAssignCourse>()
        .WithMany()
        .HasForeignKey(c => c.teaching_assign_course_id)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
