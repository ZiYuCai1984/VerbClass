using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
{
    public void Configure(EntityTypeBuilder<CourseOffering> builder)
    {
        builder.ToTable($"{AcademicConsts.DbTablePrefix}CourseOfferings", AcademicConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AcademicTermId)
            .IsRequired();

        builder.Property(x => x.CourseDefinitionId)
            .IsRequired();

        builder.Property(x => x.OfferingCode)
            .IsRequired()
            .HasMaxLength(CourseOfferingConsts.MaxOfferingCodeLength);

        builder.Property(x => x.CourseCodeSnapshot)
            .IsRequired()
            .HasMaxLength(CourseOfferingConsts.MaxCourseCodeSnapshotLength);

        builder.Property(x => x.CourseNameSnapshot)
            .IsRequired()
            .HasMaxLength(CourseOfferingConsts.MaxCourseNameSnapshotLength);

        builder.HasIndex(x => new { x.TenantId, x.AcademicTermId, x.OfferingCode })
            .IsUnique();

        builder.HasOne<AcademicTerm>()
            .WithMany()
            .HasForeignKey(x => x.AcademicTermId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CourseDefinition>()
            .WithMany()
            .HasForeignKey(x => x.CourseDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.ScheduleSlots, slotBuilder =>
        {
            slotBuilder.ToTable($"{AcademicConsts.DbTablePrefix}CourseOfferingScheduleSlots", AcademicConsts.DbSchema);

            slotBuilder.WithOwner()
                .HasForeignKey("CourseOfferingId");

            slotBuilder.Property(x => x.Weekday)
                .IsRequired();

            slotBuilder.Property(x => x.PeriodNo)
                .IsRequired();

            slotBuilder.HasKey(
                "CourseOfferingId",
                nameof(CourseOfferingScheduleSlot.Weekday),
                nameof(CourseOfferingScheduleSlot.PeriodNo)
            );
        });

        builder.Navigation(x => x.ScheduleSlots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
