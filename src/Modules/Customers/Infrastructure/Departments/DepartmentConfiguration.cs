using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Customers.Domain.Departments;

namespace Customers.Infrastructure.Departments;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departamentos");
        builder.HasKey(x => x.DepartmentId);
        builder.Property(x => x.DepartmentId).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("nombre");
        builder.Property(x => x.DaneCode).HasColumnName("codigo_dane");
        builder.Property(x => x.HomologatedCode).HasColumnName("codigo_homologado");
    }
}