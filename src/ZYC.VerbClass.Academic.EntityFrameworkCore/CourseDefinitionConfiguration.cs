using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseDefinitionConfiguration : IEntityTypeConfiguration<CourseDefinition>
{
    public void Configure(EntityTypeBuilder<CourseDefinition> builder)
    {
        builder.ToTable(
            AcademicDbProperties.DbTablePrefix + "CourseDefinitions",
            AcademicDbProperties.DbSchema
        );

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(CourseDefinitionConsts.MaxCodeLength);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(CourseDefinitionConsts.MaxNameLength);

        builder.Property(x => x.ShortName)
            .HasMaxLength(CourseDefinitionConsts.MaxShortNameLength);

        builder.Property(x => x.Description)
            .HasMaxLength(CourseDefinitionConsts.MaxDescriptionLength);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}
