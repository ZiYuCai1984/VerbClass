using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Application.Contracts.UserProfiles;

public class UpdateUserSettingsInput
{
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Family name")]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Given name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone")]
    public string? PhoneNumber { get; set; }

    [StringLength(UserProfileConsts.MaxNamePartLength)]
    [Display(Name = "Family name (kanji)")]
    public string? SurnameKanji { get; set; }

    [StringLength(UserProfileConsts.MaxNamePartLength)]
    [Display(Name = "Given name (kanji)")]
    public string? NameKanji { get; set; }

    [StringLength(UserProfileConsts.MaxNamePartLength)]
    [Display(Name = "Family name (kana)")]
    public string? SurnameKana { get; set; }

    [StringLength(UserProfileConsts.MaxNamePartLength)]
    [Display(Name = "Given name (kana)")]
    public string? NameKana { get; set; }

    [StringLength(UserProfileConsts.MaxNamePartLength)]
    [Display(Name = "Family name (romanized)")]
    public string? SurnameRomanized { get; set; }

    [StringLength(UserProfileConsts.MaxNamePartLength)]
    [Display(Name = "Given name (romanized)")]
    public string? NameRomanized { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Birth date")]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Gender")]
    public Gender? Gender { get; set; }

    [StringLength(UserProfileConsts.MaxBloodTypeLength)]
    [Display(Name = "Blood type")]
    public string? BloodType { get; set; }

    [StringLength(UserProfileConsts.MaxNationalityLength)]
    [Display(Name = "Nationality")]
    public string? Nationality { get; set; }

    [StringLength(UserProfileConsts.MaxCountryLength)]
    [Display(Name = "Country")]
    public string? Country { get; set; }

    [StringLength(UserProfileConsts.MaxPrefectureLength)]
    [Display(Name = "Prefecture / State")]
    public string? Prefecture { get; set; }

    [StringLength(UserProfileConsts.MaxCityLength)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(UserProfileConsts.MaxStreetLength)]
    [Display(Name = "Street")]
    public string? Street { get; set; }

    [StringLength(UserProfileConsts.MaxPostalCodeLength)]
    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    [Range(UserProfileConsts.MinEnrollmentYear, UserProfileConsts.MaxEnrollmentYear)]
    [Display(Name = "Enrollment year")]
    public int? EnrollmentYear { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    public string? ConfirmNewPassword { get; set; }
}
