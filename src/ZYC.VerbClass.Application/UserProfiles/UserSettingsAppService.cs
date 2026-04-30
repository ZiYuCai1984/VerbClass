using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Application.IdentityUsers;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.Application.UserProfiles;

[Authorize(VerbClassPermissions.UserSettings.Access)]
public class UserSettingsAppService : VerbClassAppService, IUserSettingsAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<UserProfile, Guid> _userProfileRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IUserAvatarAppService _userAvatarAppService;

    public UserSettingsAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<UserProfile, Guid> userProfileRepository,
        IdentityUserManager userManager,
        IUserAvatarAppService userAvatarAppService)
    {
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userManager = userManager;
        _userAvatarAppService = userAvatarAppService;
    }

    public async Task<UserSettingsDto> GetAsync()
    {
        var user = await GetCurrentUserAsync();
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == user.Id);

        return VerbClassApplicationMappers.ToUserSettingsDto(
            user,
            profile,
            (await _userManager.GetRolesAsync(user)).ToArray(),
            VerbClassApplicationMappers.ToUpdateUserSettingsInput(user, profile)
        );
    }

    [Authorize(VerbClassPermissions.UserSettings.Update)]
    public async Task SaveAsync(UpdateUserSettingsInput input)
    {
        var user = await GetCurrentUserAsync();
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == user.Id);

        NormalizeInput(input);
        await ValidateInputAsync(user, profile, input);

        user.SetUserNameWithoutValidation(input.UserName, NormalizeLookupValue(input.UserName));
        user.SetEmailWithoutValidation(input.Email, NormalizeLookupValue(input.Email));
        user.Surname = input.Surname;
        user.Name = input.Name;
        user.SetPhoneNumber(
            input.PhoneNumber ?? string.Empty,
            string.Equals(user.PhoneNumber, input.PhoneNumber, StringComparison.Ordinal) &&
            user.PhoneNumberConfirmed
        );

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw CreateValidationException(updateResult, "User settings could not be saved.");
        }

        await SaveProfileAsync(user, profile, input);

        if (RequiresPasswordChange(input))
        {
            var passwordResult = await _userManager.ChangePasswordAsync(
                user,
                input.CurrentPassword!,
                input.NewPassword!
            );

            if (!passwordResult.Succeeded)
            {
                throw CreateValidationException(passwordResult, "User settings could not be saved.");
            }
        }
    }

    [Authorize(VerbClassPermissions.UserSettings.Update)]
    public async Task UploadAvatarAsync(IRemoteStreamContent avatar)
    {
        var user = await GetCurrentUserAsync();
        await _userAvatarAppService.UploadAsync(user.Id, avatar);
    }

    [Authorize(VerbClassPermissions.UserSettings.Update)]
    public async Task RemoveAvatarAsync()
    {
        var user = await GetCurrentUserAsync();
        await _userAvatarAppService.RemoveAsync(user.Id);
    }

    private async Task<IdentityUser> GetCurrentUserAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException();
        }

        return await _userRepository.FindAsync(CurrentUser.Id.Value)
               ?? throw new UserFriendlyException("Current user was not found.");
    }

    private Task ValidateInputAsync(
        IdentityUser user,
        UserProfile? profile,
        UpdateUserSettingsInput input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (input.BirthDate.HasValue && input.BirthDate.Value.Date > DateTime.Today)
        {
            validationErrors.Add(new ValidationResult(
                "Birth date cannot be in the future.",
                [nameof(input.BirthDate)]
            ));
        }

        if (profile is null && HasProfileValues(input) && !(user.TenantId ?? CurrentTenant.Id).HasValue)
        {
            validationErrors.Add(new ValidationResult(
                "Profile settings require a tenant context."
            ));
        }

        if (RequiresPasswordChange(input))
        {
            if (string.IsNullOrWhiteSpace(input.CurrentPassword))
            {
                validationErrors.Add(new ValidationResult(
                    "Current password is required.",
                    [nameof(input.CurrentPassword)]
                ));
            }

            if (string.IsNullOrWhiteSpace(input.NewPassword))
            {
                validationErrors.Add(new ValidationResult(
                    "New password is required.",
                    [nameof(input.NewPassword)]
                ));
            }

            if (string.IsNullOrWhiteSpace(input.ConfirmNewPassword))
            {
                validationErrors.Add(new ValidationResult(
                    "Confirm new password is required.",
                    [nameof(input.ConfirmNewPassword)]
                ));
            }
            else if (!string.Equals(input.NewPassword, input.ConfirmNewPassword, StringComparison.Ordinal))
            {
                validationErrors.Add(new ValidationResult(
                    "Passwords do not match.",
                    [nameof(input.ConfirmNewPassword)]
                ));
            }
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("User settings are invalid.", validationErrors);
        }

        return Task.CompletedTask;
    }

    private async Task SaveProfileAsync(
        IdentityUser user,
        UserProfile? profile,
        UpdateUserSettingsInput input)
    {
        if (profile is null && !HasProfileValues(input))
        {
            return;
        }

        var isNewProfile = profile is null;
        if (isNewProfile)
        {
            var tenantId = user.TenantId ?? CurrentTenant.Id;
            if (!tenantId.HasValue)
            {
                throw new AbpValidationException(
                    "User settings are invalid.",
                    [new ValidationResult("Profile settings require a tenant context.")]
                );
            }

            profile = new UserProfile(GuidGenerator.Create(), tenantId.Value, user.Id);
        }

        var targetProfile = profile ?? throw new InvalidOperationException("User profile was not initialized.");

        try
        {
            targetProfile.ChangeName(
                input.SurnameKanji,
                input.NameKanji,
                input.SurnameKana,
                input.NameKana,
                input.SurnameRomanized,
                input.NameRomanized
            );
            targetProfile.ChangeBirthDate(input.BirthDate);
            targetProfile.ChangeGender(input.Gender);
            targetProfile.ChangeBloodType(input.BloodType);
            targetProfile.ChangeNationality(input.Nationality);
            targetProfile.ChangeAddress(
                input.Country,
                input.Prefecture,
                input.City,
                input.Street,
                input.PostalCode
            );
            targetProfile.ChangeEnrollmentYear(input.EnrollmentYear);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }

        if (isNewProfile)
        {
            await _userProfileRepository.InsertAsync(targetProfile, true);
        }
        else
        {
            await _userProfileRepository.UpdateAsync(targetProfile, true);
        }
    }

    private static void NormalizeInput(UpdateUserSettingsInput input)
    {
        input.UserName = input.UserName.Trim();
        input.Surname = input.Surname.Trim();
        input.Name = input.Name.Trim();
        input.Email = input.Email.Trim();
        input.PhoneNumber = NormalizeOptional(input.PhoneNumber);
        input.SurnameKanji = NormalizeOptional(input.SurnameKanji);
        input.NameKanji = NormalizeOptional(input.NameKanji);
        input.SurnameKana = NormalizeOptional(input.SurnameKana);
        input.NameKana = NormalizeOptional(input.NameKana);
        input.SurnameRomanized = NormalizeOptional(input.SurnameRomanized);
        input.NameRomanized = NormalizeOptional(input.NameRomanized);
        input.BloodType = NormalizeOptional(input.BloodType);
        input.Nationality = NormalizeOptional(input.Nationality);
        input.Country = NormalizeOptional(input.Country);
        input.Prefecture = NormalizeOptional(input.Prefecture);
        input.City = NormalizeOptional(input.City);
        input.Street = NormalizeOptional(input.Street);
        input.PostalCode = NormalizeOptional(input.PostalCode);
    }

    private static bool RequiresPasswordChange(UpdateUserSettingsInput input)
    {
        return !string.IsNullOrWhiteSpace(input.CurrentPassword) ||
               !string.IsNullOrWhiteSpace(input.NewPassword) ||
               !string.IsNullOrWhiteSpace(input.ConfirmNewPassword);
    }

    private static bool HasProfileValues(UpdateUserSettingsInput input)
    {
        return input.BirthDate.HasValue ||
               input.Gender.HasValue ||
               input.EnrollmentYear.HasValue ||
               !string.IsNullOrWhiteSpace(input.SurnameKanji) ||
               !string.IsNullOrWhiteSpace(input.NameKanji) ||
               !string.IsNullOrWhiteSpace(input.SurnameKana) ||
               !string.IsNullOrWhiteSpace(input.NameKana) ||
               !string.IsNullOrWhiteSpace(input.SurnameRomanized) ||
               !string.IsNullOrWhiteSpace(input.NameRomanized) ||
               !string.IsNullOrWhiteSpace(input.BloodType) ||
               !string.IsNullOrWhiteSpace(input.Nationality) ||
               !string.IsNullOrWhiteSpace(input.Country) ||
               !string.IsNullOrWhiteSpace(input.Prefecture) ||
               !string.IsNullOrWhiteSpace(input.City) ||
               !string.IsNullOrWhiteSpace(input.Street) ||
               !string.IsNullOrWhiteSpace(input.PostalCode);
    }

    private static AbpValidationException CreateValidationException(IdentityResult result, string message)
    {
        var validationErrors = result.Errors
            .Select(error =>
            {
                var fieldName = error.Code switch
                {
                    "DuplicateUserName" or "InvalidUserName" => nameof(UpdateUserSettingsInput.UserName),
                    "DuplicateEmail" or "InvalidEmail" => nameof(UpdateUserSettingsInput.Email),
                    "PasswordMismatch" => nameof(UpdateUserSettingsInput.CurrentPassword),
                    var passwordCode when passwordCode.StartsWith("Password", StringComparison.OrdinalIgnoreCase) =>
                        nameof(UpdateUserSettingsInput.NewPassword),
                    _ => null
                };

                return fieldName is null
                    ? new ValidationResult(error.Description)
                    : new ValidationResult(error.Description, [fieldName]);
            })
            .ToList();

        return new AbpValidationException(message, validationErrors);
    }

    private static AbpValidationException CreateValidationException(BusinessException exception)
    {
        return exception.Code switch
        {
            UserProfileErrorCodes.InvalidBirthDate => new AbpValidationException(
                "User settings are invalid.",
                [new ValidationResult("Birth date cannot be in the future.", [nameof(UpdateUserSettingsInput.BirthDate)])]
            ),
            UserProfileErrorCodes.InvalidEnrollmentYear => new AbpValidationException(
                "User settings are invalid.",
                [new ValidationResult(
                    $"Enrollment year must be between {UserProfileConsts.MinEnrollmentYear} and {UserProfileConsts.MaxEnrollmentYear}.",
                    [nameof(UpdateUserSettingsInput.EnrollmentYear)]
                )]
            ),
            UserProfileErrorCodes.InvalidGender => new AbpValidationException(
                "User settings are invalid.",
                [new ValidationResult("Gender value is invalid.", [nameof(UpdateUserSettingsInput.Gender)])]
            ),
            _ => new AbpValidationException(
                string.IsNullOrWhiteSpace(exception.Message) ? "User settings are invalid." : exception.Message,
                [new ValidationResult(
                    string.IsNullOrWhiteSpace(exception.Message) ? "User settings are invalid." : exception.Message
                )]
            )
        };
    }
    private static string NormalizeLookupValue(string value)
    {
        return value.ToUpperInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
