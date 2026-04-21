using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Memberships;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseMembershipConfiguration : IEntityTypeConfiguration<CourseMembership>
{
    public void Configure(EntityTypeBuilder<CourseMembership> builder)
    {
        builder.ToTable(
            AcademicDbProperties.DbTablePrefix + "CourseMemberships",
            AcademicDbProperties.DbSchema
        );

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CourseOfferingId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.CourseOfferingId, x.UserId })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.CourseOfferingId, x.Status });

        builder.HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
