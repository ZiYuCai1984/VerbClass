using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
{
    public void Configure(EntityTypeBuilder<CourseOffering> builder)
    {
        builder.ToTable(
            AcademicDbProperties.DbTablePrefix + "CourseOfferings",
            AcademicDbProperties.DbSchema
        );

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CourseDefinitionId)
            .IsRequired();

        builder.Property(x => x.AcademicYear)
            .HasColumnName("TermAcademicYear")
            .IsRequired();

        builder.Property(x => x.TermName)
            .HasColumnName("TermTermName")
            .HasMaxLength(CourseOfferingConsts.MaxTermNameLength)
            .IsRequired();

        builder.Property(x => x.ScheduleDayOfWeek)
            .HasColumnName("ScheduleDayOfWeek");

        builder.Property(x => x.ScheduleStartTime)
            .HasColumnName("ScheduleStartTime");

        builder.Property(x => x.ScheduleEndTime)
            .HasColumnName("ScheduleEndTime");

        builder.Property(x => x.ScheduleLocation)
            .HasColumnName("ScheduleLocation")
            .HasMaxLength(CourseOfferingConsts.MaxLocationLength);

        builder.Property(x => x.EnrollmentStartsAt)
            .HasColumnName("EnrollmentOpensAt")
            .IsRequired();

        builder.Property(x => x.EnrollmentEndsAt)
            .HasColumnName("EnrollmentClosesAt")
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.IsLocked)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.CourseDefinitionId });

        builder.HasOne<CourseDefinition>()
            .WithMany()
            .HasForeignKey(x => x.CourseDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
