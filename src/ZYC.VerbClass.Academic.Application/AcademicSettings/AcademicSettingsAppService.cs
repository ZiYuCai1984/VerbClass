using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Domain.Shared;
using AcademicSettingNames = ZYC.VerbClass.Academic.Domain.AcademicSettings;

namespace ZYC.VerbClass.Academic.Application.AcademicSettings;

[Authorize]
public class AcademicSettingsAppService : ApplicationService, IAcademicSettingsAppService
{
    private readonly IAcademicTermRepository _academicTermRepository;
    private readonly ISettingManager _settingManager;

    public AcademicSettingsAppService(
        IAcademicTermRepository academicTermRepository,
        ISettingManager settingManager)
    {
        _academicTermRepository = academicTermRepository;
        _settingManager = settingManager;
    }

    [Authorize(VerbClassPermissions.TenantSettings.Access)]
    public async Task<AcademicSettingsDto> GetAsync()
    {
        EnsureTenantContext();

        var termOptions = await GetTermOptionsAsync();
        return new AcademicSettingsDto
        {
            CurrentTermId = await GetConfiguredCurrentTermIdAsync(termOptions),
            TermOptions = termOptions,
            Permissions = await GetPermissionsAsync()
        };
    }

    public async Task<Guid?> GetCurrentTermIdAsync()
    {
        EnsureTenantContext();

        return await GetConfiguredCurrentTermIdAsync(await GetTermOptionsAsync());
    }

    [Authorize(VerbClassPermissions.TenantSettings.Access)]
    [Authorize(VerbClassPermissions.TenantSettings.UpdateAcademicSettings)]
    public async Task<AcademicSettingsDto> SaveCurrentTermAsync(UpdateCurrentAcademicTermInput input)
    {
        EnsureTenantContext();
        await ValidateCurrentTermAsync(input);

        await _settingManager.SetForCurrentTenantAsync(
            AcademicSettingNames.CurrentTermId,
            input.CurrentTermId?.ToString("D") ?? string.Empty);

        return await GetAsync();
    }

    private async Task<AcademicSettingsTermOptionDto[]> GetTermOptionsAsync()
    {
        return (await _academicTermRepository.GetListAsync())
            .OrderByDescending(x => x.AcademicYear)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Select(term => new AcademicSettingsTermOptionDto
            {
                Id = term.Id,
                AcademicYear = term.AcademicYear,
                Code = term.Code,
                Name = term.Name
            })
            .ToArray();
    }

    private async Task<Guid?> GetConfiguredCurrentTermIdAsync(AcademicSettingsTermOptionDto[] termOptions)
    {
        var value = await _settingManager.GetOrNullForCurrentTenantAsync(AcademicSettingNames.CurrentTermId);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!Guid.TryParse(value, out var currentTermId) || currentTermId == Guid.Empty)
        {
            throw new UserFriendlyException(
                $"Setting '{AcademicSettingNames.CurrentTermId}' has invalid value '{value}'.");
        }

        if (termOptions.All(x => x.Id != currentTermId))
        {
            throw new UserFriendlyException("Configured current academic term was not found.");
        }

        return currentTermId;
    }

    private async Task<AcademicSettingsPermissionsDto> GetPermissionsAsync()
    {
        return new AcademicSettingsPermissionsDto
        {
            CanUpdateCurrentTerm = await AuthorizationService.IsGrantedAsync(
                VerbClassPermissions.TenantSettings.UpdateAcademicSettings)
        };
    }

    private async Task ValidateCurrentTermAsync(UpdateCurrentAcademicTermInput input)
    {
        if (!input.CurrentTermId.HasValue)
        {
            return;
        }

        if (input.CurrentTermId.Value == Guid.Empty)
        {
            throw new AbpValidationException(
                "Current academic term is invalid.",
                [new ValidationResult("Current academic term is invalid.", [nameof(input.CurrentTermId)])]);
        }

        if (await _academicTermRepository.FindAsync(input.CurrentTermId.Value) is null)
        {
            throw new AbpValidationException(
                "Current academic term was not found.",
                [new ValidationResult("Current academic term was not found.", [nameof(input.CurrentTermId)])]);
        }
    }

    private void EnsureTenantContext()
    {
        if (!CurrentTenant.Id.HasValue)
        {
            throw new UserFriendlyException("Academic settings require a tenant context.");
        }
    }
}
