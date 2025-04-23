using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Elearning.Modules.Program.Infrastructure.Program;

internal sealed class CourseMaterialConfiguration : IEntityTypeConfiguration<CourseMaterial>
{
  public void Configure(EntityTypeBuilder<CourseMaterial> builder)
  {
    builder.ToTable("table_course_materials");
    builder.HasKey(c => c.id);

    builder.Property(c => c.title).HasMaxLength(255).IsRequired();
    builder.Property(c => c.description).HasColumnType("text");
    builder.Property(c => c.file_url).IsRequired();
    builder.Property(c => c.file_type).HasMaxLength(50).IsRequired();
    builder.Property(c => c.file_size).IsRequired();
    builder.Property(c => c.is_published).HasDefaultValue(false);
    builder.Property(c => c.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
    builder.Property(c => c.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
    builder.Property(c => c.youtube_video_id).HasMaxLength(50);
    builder.Property(c => c.content_type).HasMaxLength(50);

    // Foreign key relationship
    builder.HasOne<Courses>()
        .WithMany()
        .HasForeignKey(c => c.course_id)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
