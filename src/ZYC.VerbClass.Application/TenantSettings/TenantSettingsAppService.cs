using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Identity.Settings;
using Volo.Abp.SettingManagement;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.TenantSettings;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Application.TenantSettings;

[Authorize(VerbClassPermissions.TenantSettings.Access)]
public class TenantSettingsAppService : VerbClassAppService, ITenantSettingsAppService
{
    private readonly ISettingManager _settingManager;

    public TenantSettingsAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task<TenantSettingsDto> GetAsync()
    {
        EnsureTenantContext();

        return new TenantSettingsDto
        {
            PasswordPolicy = await GetPasswordPolicyAsync(),
            Permissions = await GetPermissionsAsync()
        };
    }

    [Authorize(VerbClassPermissions.TenantSettings.UpdatePasswordPolicy)]
    public async Task<TenantPasswordPolicyDto> SavePasswordPolicyAsync(UpdateTenantPasswordPolicyInput input)
    {
        EnsureTenantContext();
        ValidatePasswordPolicy(input);

        await _settingManager.SetForCurrentTenantAsync(
            IdentitySettingNames.Password.RequireDigit,
            input.RequireDigit.ToString());
        await _settingManager.SetForCurrentTenantAsync(
            IdentitySettingNames.Password.RequireLowercase,
            input.RequireLowercase.ToString());
        await _settingManager.SetForCurrentTenantAsync(
            IdentitySettingNames.Password.RequireUppercase,
            input.RequireUppercase.ToString());
        await _settingManager.SetForCurrentTenantAsync(
            IdentitySettingNames.Password.RequireNonAlphanumeric,
            input.RequireNonAlphanumeric.ToString());
        await _settingManager.SetForCurrentTenantAsync(
            IdentitySettingNames.Password.RequiredLength,
            input.RequiredLength.ToString(CultureInfo.InvariantCulture));
        await _settingManager.SetForCurrentTenantAsync(
            IdentitySettingNames.Password.RequiredUniqueChars,
            input.RequiredUniqueChars.ToString(CultureInfo.InvariantCulture));

        return await GetPasswordPolicyAsync();
    }

    private async Task<TenantPasswordPolicyDto> GetPasswordPolicyAsync()
    {
        return new TenantPasswordPolicyDto
        {
            RequireDigit = await GetRequiredBooleanSettingAsync(IdentitySettingNames.Password.RequireDigit),
            RequireLowercase = await GetRequiredBooleanSettingAsync(IdentitySettingNames.Password.RequireLowercase),
            RequireUppercase = await GetRequiredBooleanSettingAsync(IdentitySettingNames.Password.RequireUppercase),
            RequireNonAlphanumeric = await GetRequiredBooleanSettingAsync(IdentitySettingNames.Password.RequireNonAlphanumeric),
            RequiredLength = await GetRequiredIntegerSettingAsync(IdentitySettingNames.Password.RequiredLength),
            RequiredUniqueChars = await GetRequiredIntegerSettingAsync(IdentitySettingNames.Password.RequiredUniqueChars)
        };
    }

    private async Task<TenantSettingsPermissionsDto> GetPermissionsAsync()
    {
        return new TenantSettingsPermissionsDto
        {
            CanUpdatePasswordPolicy = await AuthorizationService.IsGrantedAsync(
                VerbClassPermissions.TenantSettings.UpdatePasswordPolicy)
        };
    }

    private async Task<bool> GetRequiredBooleanSettingAsync(string name)
    {
        var value = await _settingManager.GetOrNullForCurrentTenantAsync(name);
        if (value is null)
        {
            throw CreateMissingSettingException(name);
        }

        if (!bool.TryParse(value, out var result))
        {
            throw CreateInvalidSettingException(name, value);
        }

        return result;
    }

    private async Task<int> GetRequiredIntegerSettingAsync(string name)
    {
        var value = await _settingManager.GetOrNullForCurrentTenantAsync(name);
        if (value is null)
        {
            throw CreateMissingSettingException(name);
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw CreateInvalidSettingException(name, value);
        }

        return result;
    }

    private void EnsureTenantContext()
    {
        if (!CurrentTenant.Id.HasValue)
        {
            throw new UserFriendlyException("Tenant settings require a tenant context.");
        }
    }

    private static void ValidatePasswordPolicy(UpdateTenantPasswordPolicyInput input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (input.RequiredUniqueChars > input.RequiredLength)
        {
            validationErrors.Add(new ValidationResult(
                "Required unique characters cannot exceed the required password length.",
                [nameof(input.RequiredUniqueChars)]));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Password policy is invalid.", validationErrors);
        }
    }

    private static UserFriendlyException CreateMissingSettingException(string name)
    {
        return new UserFriendlyException($"Required setting '{name}' is not defined.");
    }

    private static UserFriendlyException CreateInvalidSettingException(string name, string value)
    {
        return new UserFriendlyException($"Setting '{name}' has invalid value '{value}'.");
    }
}
