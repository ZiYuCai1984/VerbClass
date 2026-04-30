using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class AcademicTermConfiguration : IEntityTypeConfiguration<AcademicTerm>
{
    public void Configure(EntityTypeBuilder<AcademicTerm> builder)
    {
        builder.ToTable($"{AcademicConsts.DbTablePrefix}Terms", AcademicConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AcademicYear)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(AcademicTermConsts.MaxCodeLength);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(AcademicTermConsts.MaxNameLength);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.IsLocked)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.AcademicYear, x.Code })
            .IsUnique();

        builder.OwnsMany(x => x.Periods, periodBuilder =>
        {
            periodBuilder.ToTable($"{AcademicConsts.DbTablePrefix}TermPeriods", AcademicConsts.DbSchema);

            periodBuilder.WithOwner()
                .HasForeignKey("AcademicTermId");

            periodBuilder.Property(x => x.PeriodNo)
                .ValueGeneratedNever()
                .IsRequired();

            periodBuilder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(AcademicTermConsts.MaxPeriodLabelLength);

            periodBuilder.Property(x => x.StartTime)
                .IsRequired();

            periodBuilder.Property(x => x.EndTime)
                .IsRequired();

            periodBuilder.HasKey("AcademicTermId", nameof(TermPeriodDefinition.PeriodNo));
        });

        builder.Navigation(x => x.Periods)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
