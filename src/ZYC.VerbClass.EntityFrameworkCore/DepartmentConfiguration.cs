using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.Departments;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.EntityFrameworkCore;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable($"{VerbClassConsts.DbTablePrefix}Departments");

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(DepartmentConsts.MaxCodeLength);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(DepartmentConsts.MaxNameLength);

        builder.Property(x => x.ShortName)
            .HasMaxLength(DepartmentConsts.MaxShortNameLength);

        builder.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(DepartmentConsts.MaxPathLength);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CanAssignUsers)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.ParentDepartmentId });

        builder.HasIndex(x => new { x.TenantId, x.Path });
    }
}
