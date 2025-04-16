using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ProgramUnitConfiguration : IEntityTypeConfiguration<ProgramUnit>
{
  public void Configure(EntityTypeBuilder<ProgramUnit> builder)
  {
    builder.ToTable("table_programs");
    builder.HasKey(p => p.id);
    builder.Property(p => p.name).HasMaxLength(100).IsRequired();
    builder.Property(p => p.code).HasMaxLength(20).IsRequired();
    builder
      .HasOne(p => p.Department)
      .WithMany(d => d.Programs)
      .HasForeignKey(p => p.department_id)
      .HasConstraintName("fk_programunit_department");
  }
}
