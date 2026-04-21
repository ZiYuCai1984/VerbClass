using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using ZYC.VerbClass.Domain.AppFiles;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.EntityFrameworkCore;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable($"{VerbClassConsts.DbTablePrefix}UserProfiles");

        builder.ConfigureByConvention();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Gender);

        builder.Property(x => x.BloodType)
            .HasMaxLength(UserProfileConsts.MaxBloodTypeLength);

        builder.Property(x => x.Nationality)
            .HasMaxLength(UserProfileConsts.MaxNationalityLength);

        builder.HasIndex(x => x.AvatarFileId);

        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique();

        builder.HasOne<AppFile>()
            .WithMany()
            .HasForeignKey(x => x.AvatarFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(x => x.NameInfo, nameBuilder =>
        {
            nameBuilder.Property(x => x.SurnameKanji)
                .HasColumnName(nameof(PersonNameInfo.SurnameKanji))
                .HasMaxLength(UserProfileConsts.MaxNamePartLength);

            nameBuilder.Property(x => x.NameKanji)
                .HasColumnName(nameof(PersonNameInfo.NameKanji))
                .HasMaxLength(UserProfileConsts.MaxNamePartLength);

            nameBuilder.Property(x => x.SurnameKana)
                .HasColumnName(nameof(PersonNameInfo.SurnameKana))
                .HasMaxLength(UserProfileConsts.MaxNamePartLength);

            nameBuilder.Property(x => x.NameKana)
                .HasColumnName(nameof(PersonNameInfo.NameKana))
                .HasMaxLength(UserProfileConsts.MaxNamePartLength);

            nameBuilder.Property(x => x.SurnameRomanized)
                .HasColumnName(nameof(PersonNameInfo.SurnameRomanized))
                .HasMaxLength(UserProfileConsts.MaxNamePartLength);

            nameBuilder.Property(x => x.NameRomanized)
                .HasColumnName(nameof(PersonNameInfo.NameRomanized))
                .HasMaxLength(UserProfileConsts.MaxNamePartLength);
        });

        builder.Navigation(x => x.NameInfo)
            .IsRequired();

        builder.OwnsOne(x => x.AddressInfo, addressBuilder =>
        {
            addressBuilder.Property(x => x.Country)
                .HasColumnName(nameof(AddressInfo.Country))
                .HasMaxLength(UserProfileConsts.MaxCountryLength);

            addressBuilder.Property(x => x.Prefecture)
                .HasColumnName(nameof(AddressInfo.Prefecture))
                .HasMaxLength(UserProfileConsts.MaxPrefectureLength);

            addressBuilder.Property(x => x.City)
                .HasColumnName(nameof(AddressInfo.City))
                .HasMaxLength(UserProfileConsts.MaxCityLength);

            addressBuilder.Property(x => x.Street)
                .HasColumnName(nameof(AddressInfo.Street))
                .HasMaxLength(UserProfileConsts.MaxStreetLength);

            addressBuilder.Property(x => x.PostalCode)
                .HasColumnName(nameof(AddressInfo.PostalCode))
                .HasMaxLength(UserProfileConsts.MaxPostalCodeLength);
        });

        builder.Navigation(x => x.AddressInfo)
            .IsRequired();
    }
}
