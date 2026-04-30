using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseOfferingParticipantConfiguration : IEntityTypeConfiguration<CourseOfferingParticipant>
{
    public void Configure(EntityTypeBuilder<CourseOfferingParticipant> builder)
    {
        builder.ToTable($"{AcademicConsts.DbTablePrefix}CourseOfferingParticipants", AcademicConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CourseOfferingId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.CourseOfferingId, x.UserId })
            .HasFilter("\"IsDeleted\" = 0")
            .IsUnique();

        builder.HasOne<CourseOffering>()
            .WithMany()
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
