using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.AppFiles;

namespace ZYC.VerbClass.EntityFrameworkCore;

public class AppFileConfiguration : IEntityTypeConfiguration<AppFile>
{
    public void Configure(EntityTypeBuilder<AppFile> builder)
    {
        builder.ToTable($"{VerbClassConsts.DbTablePrefix}Files");

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Size)
            .IsRequired();

        builder.Property(x => x.BlobName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.TenantId, x.BlobName })
            .IsUnique();
    }
}
