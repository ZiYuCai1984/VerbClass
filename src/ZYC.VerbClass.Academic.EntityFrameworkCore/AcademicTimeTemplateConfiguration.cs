using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class AcademicTimeTemplateConfiguration : IEntityTypeConfiguration<AcademicTimeTemplate>
{
    public void Configure(EntityTypeBuilder<AcademicTimeTemplate> builder)
    {
        builder.ToTable($"{AcademicConsts.DbTablePrefix}TimeTemplates", AcademicConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(AcademicTimeTemplateConsts.MaxCodeLength);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(AcademicTimeTemplateConsts.MaxNameLength);

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();

        builder.OwnsMany(x => x.Periods, periodBuilder =>
        {
            periodBuilder.ToTable($"{AcademicConsts.DbTablePrefix}TimeTemplatePeriods", AcademicConsts.DbSchema);

            periodBuilder.WithOwner()
                .HasForeignKey("AcademicTimeTemplateId");

            periodBuilder.Property(x => x.PeriodNo)
                .ValueGeneratedNever()
                .IsRequired();

            periodBuilder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(AcademicTimeTemplateConsts.MaxPeriodLabelLength);

            periodBuilder.Property(x => x.StartTime)
                .IsRequired();

            periodBuilder.Property(x => x.EndTime)
                .IsRequired();

            periodBuilder.HasKey("AcademicTimeTemplateId", nameof(TimeTemplatePeriodDefinition.PeriodNo));
        });

        builder.Navigation(x => x.Periods)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
