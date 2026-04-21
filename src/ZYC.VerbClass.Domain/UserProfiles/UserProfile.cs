using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.UserProfiles;

public class UserProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    protected UserProfile()
    {
    }

    public UserProfile(
        Guid id,
        Guid tenantId,
        Guid userId,
        PersonNameInfo? nameInfo = null,
        DateTime? birthDate = null,
        Gender? gender = null,
        string? bloodType = null,
        string? nationality = null,
        AddressInfo? addressInfo = null,
        Guid? avatarFileId = null,
        int? enrollmentYear = null) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        SetUserId(userId);

        ChangeName(nameInfo ?? new PersonNameInfo());
        ChangeBirthDate(birthDate);
        ChangeGender(gender);
        ChangeBloodType(bloodType);
        ChangeNationality(nationality);
        ChangeAddress(addressInfo ?? new AddressInfo());
        ChangeAvatar(avatarFileId);
        ChangeEnrollmentYear(enrollmentYear);
    }

    public Guid? TenantId { get; protected set; }

    public Guid UserId { get; private set; }

    public PersonNameInfo NameInfo { get; private set; } = new();

    public DateTime? BirthDate { get; private set; }

    public Gender? Gender { get; private set; }

    public string? BloodType { get; private set; }

    public string? Nationality { get; private set; }

    public AddressInfo AddressInfo { get; private set; } = new();

    public Guid? AvatarFileId { get; private set; }

    public int? EnrollmentYear { get; private set; }

    public void ChangeName(
        string? surnameKanji,
        string? nameKanji,
        string? surnameKana,
        string? nameKana,
        string? surnameRomanized,
        string? nameRomanized)
    {
        NameInfo.Change(
            surnameKanji,
            nameKanji,
            surnameKana,
            nameKana,
            surnameRomanized,
            nameRomanized
        );
    }

    public void ChangeName(PersonNameInfo nameInfo)
    {
        Check.NotNull(nameInfo, nameof(nameInfo));

        ChangeName(
            nameInfo.SurnameKanji,
            nameInfo.NameKanji,
            nameInfo.SurnameKana,
            nameInfo.NameKana,
            nameInfo.SurnameRomanized,
            nameInfo.NameRomanized
        );
    }

    public void ChangeBirthDate(DateTime? birthDate)
    {
        if (!birthDate.HasValue)
        {
            BirthDate = null;
            return;
        }

        var normalizedBirthDate = birthDate.Value.Date;
        if (normalizedBirthDate > DateTime.Today)
        {
            throw new BusinessException(UserProfileErrorCodes.InvalidBirthDate)
                .WithData("BirthDate", normalizedBirthDate);
        }

        BirthDate = normalizedBirthDate;
    }

    public void ChangeGender(Gender? gender)
    {
        if (gender.HasValue && !Enum.IsDefined(gender.Value))
        {
            throw new BusinessException(UserProfileErrorCodes.InvalidGender)
                .WithData("Gender", (int)gender.Value);
        }

        Gender = gender;
    }

    public void ChangeBloodType(string? bloodType)
    {
        BloodType = NormalizeText(
            bloodType,
            nameof(bloodType),
            UserProfileConsts.MaxBloodTypeLength
        );
    }

    public void ChangeNationality(string? nationality)
    {
        Nationality = NormalizeText(
            nationality,
            nameof(nationality),
            UserProfileConsts.MaxNationalityLength
        );
    }

    public void ChangeAddress(
        string? country,
        string? prefecture,
        string? city,
        string? street,
        string? postalCode)
    {
        AddressInfo.Change(country, prefecture, city, street, postalCode);
    }

    public void ChangeAddress(AddressInfo addressInfo)
    {
        Check.NotNull(addressInfo, nameof(addressInfo));

        ChangeAddress(
            addressInfo.Country,
            addressInfo.Prefecture,
            addressInfo.City,
            addressInfo.Street,
            addressInfo.PostalCode
        );
    }

    public void ChangeAvatar(Guid? avatarFileId)
    {
        if (avatarFileId.HasValue && avatarFileId.Value == Guid.Empty)
        {
            throw new BusinessException(UserProfileErrorCodes.AvatarFileIdCannotBeEmpty);
        }

        AvatarFileId = avatarFileId;
    }

    public void ChangeEnrollmentYear(int? enrollmentYear)
    {
        if (enrollmentYear.HasValue &&
            (enrollmentYear.Value < UserProfileConsts.MinEnrollmentYear ||
             enrollmentYear.Value > UserProfileConsts.MaxEnrollmentYear))
        {
            throw new BusinessException(UserProfileErrorCodes.InvalidEnrollmentYear)
                .WithData("EnrollmentYear", enrollmentYear.Value);
        }

        EnrollmentYear = enrollmentYear;
    }

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new BusinessException(UserProfileErrorCodes.UserIdCannotBeEmpty);
        }

        UserId = userId;
    }

    private static string? NormalizeText(string? value, string parameterName, int maxLength)
    {
        if (value.IsNullOrWhiteSpace())
        {
            return null;
        }

        return Check.Length(value.Trim(), parameterName, maxLength);
    }
}
