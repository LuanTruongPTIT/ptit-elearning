using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Courses>
{
  public void Configure(EntityTypeBuilder<Courses> builder)
  {
    builder.ToTable("table_courses");
    builder.HasKey(c => c.id);
    builder.Property(c => c.name).HasMaxLength(100).IsRequired();
    builder.Property(c => c.code).HasMaxLength(20).IsRequired();

    builder
      .HasMany(c => c.Programs)
      .WithMany(p => p.Courses)
      .UsingEntity<Dictionary<string, object>>(
        "table_program_courses",
        j => j
          .HasOne<ProgramUnit>()
          .WithMany()
          .HasForeignKey("program_id")
          .HasConstraintName("fk_program_courses_program_id"),
        j => j
          .HasOne<Courses>()
          .WithMany()
          .HasForeignKey("course_id")
          .HasConstraintName("fk_program_courses_course_id"),
        j =>
        {
          j.HasKey("program_id", "course_id").HasName("pk_program_courses");
          j.ToTable("table_program_courses");
        }
      );
  }
}
