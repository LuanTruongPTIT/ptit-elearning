using Elearning.Modules.Program.Domain.Program;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
  public void Configure(EntityTypeBuilder<Department> builder)
  {
    builder.ToTable("table_departments");
    builder.HasKey(d => d.id);
    builder.Property(d => d.name).HasMaxLength(100).IsRequired();
    builder.Property(d => d.code).HasMaxLength(20).IsRequired();
  }
}
