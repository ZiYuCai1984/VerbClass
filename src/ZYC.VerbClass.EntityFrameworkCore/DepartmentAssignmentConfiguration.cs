using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.Departments;

namespace ZYC.VerbClass.EntityFrameworkCore;

public class DepartmentAssignmentConfiguration : IEntityTypeConfiguration<DepartmentAssignment>
{
    public void Configure(EntityTypeBuilder<DepartmentAssignment> builder)
    {
        builder.ToTable($"{VerbClassConsts.DbTablePrefix}DepartmentAssignments");

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DepartmentId)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.UserId });
        builder.HasIndex(x => new { x.TenantId, x.DepartmentId });
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.DepartmentId, x.EffectiveFrom });

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
