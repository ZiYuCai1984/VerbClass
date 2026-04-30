using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;
using ZYC.VerbClass.Application.Contracts.TenantSettings;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Admin;

[Authorize(VerbClassPermissions.TenantSettings.Access)]
public class SettingsModel : VerbClassPageModel
{
    public SettingsModel(
        ILifetimeScope lifetimeScope,
        ITenantSettingsAppService tenantSettingsAppService,
        IAcademicSettingsAppService academicSettingsAppService) : base(lifetimeScope)
    {
        TenantSettingsAppService = tenantSettingsAppService;
        AcademicSettingsAppService = academicSettingsAppService;
    }

    protected override string PageTitle => "Settings";

    [BindProperty] public UpdateTenantPasswordPolicyInput PasswordPolicyInput { get; set; } = new();

    [BindProperty] public UpdateCurrentAcademicTermInput CurrentAcademicTermInput { get; set; } = new();

    public bool CanUpdatePasswordPolicy { get; private set; }

    public bool CanUpdateAcademicSettings { get; private set; }

    public AcademicSettingsTermOptionDto[] AcademicTermOptions { get; private set; } = [];

    public string TenantName { get; private set; } = "";

    private ITenantSettingsAppService TenantSettingsAppService { get; }

    private IAcademicSettingsAppService AcademicSettingsAppService { get; }

    public async Task OnGetAsync()
    {
        await LoadPageStateAsync(populatePasswordPolicy: true, populateCurrentAcademicTerm: true);
    }

    public async Task<IActionResult> OnPostPasswordPolicyAsync()
    {
        await LoadPageStateAsync(populatePasswordPolicy: false, populateCurrentAcademicTerm: true);

        if (!CanUpdatePasswordPolicy)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        ModelState.Clear();
        TryValidateModel(PasswordPolicyInput, nameof(PasswordPolicyInput));

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await TenantSettingsAppService.SavePasswordPolicyAsync(PasswordPolicyInput);

            ToastInfo("Password policy was saved.", "Settings");
            return RedirectToPage();
        }
        catch (AbpValidationException ex)
        {
            ModelStateValidationSupport.ApplyAbpValidationException(
                ModelState,
                ex,
                "Password policy could not be saved.",
                nameof(PasswordPolicyInput)
            );
            return Page();
        }
        catch (UserFriendlyException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCurrentAcademicTermAsync()
    {
        await LoadPageStateAsync(populatePasswordPolicy: true, populateCurrentAcademicTerm: false);

        if (!CanUpdateAcademicSettings)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        ModelState.Clear();
        TryValidateModel(CurrentAcademicTermInput, nameof(CurrentAcademicTermInput));

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await AcademicSettingsAppService.SaveCurrentTermAsync(CurrentAcademicTermInput);

            ToastInfo("Current academic term was saved.", "Settings");
            return RedirectToPage();
        }
        catch (AbpValidationException ex)
        {
            ModelStateValidationSupport.ApplyAbpValidationException(
                ModelState,
                ex,
                "Current academic term could not be saved.",
                nameof(CurrentAcademicTermInput)
            );
            return Page();
        }
        catch (UserFriendlyException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    private async Task LoadPageStateAsync(
        bool populatePasswordPolicy,
        bool populateCurrentAcademicTerm)
    {
        TenantName = CurrentTenant.Name ?? "";

        var tenantSettings = await TenantSettingsAppService.GetAsync();
        CanUpdatePasswordPolicy = tenantSettings.Permissions.CanUpdatePasswordPolicy;

        if (populatePasswordPolicy)
        {
            PasswordPolicyInput = new UpdateTenantPasswordPolicyInput
            {
                RequireDigit = tenantSettings.PasswordPolicy.RequireDigit,
                RequireLowercase = tenantSettings.PasswordPolicy.RequireLowercase,
                RequireUppercase = tenantSettings.PasswordPolicy.RequireUppercase,
                RequireNonAlphanumeric = tenantSettings.PasswordPolicy.RequireNonAlphanumeric,
                RequiredLength = tenantSettings.PasswordPolicy.RequiredLength,
                RequiredUniqueChars = tenantSettings.PasswordPolicy.RequiredUniqueChars
            };
        }

        var academicSettings = await AcademicSettingsAppService.GetAsync();
        CanUpdateAcademicSettings = academicSettings.Permissions.CanUpdateCurrentTerm;
        AcademicTermOptions = academicSettings.TermOptions;

        if (populateCurrentAcademicTerm)
        {
            CurrentAcademicTermInput = new UpdateCurrentAcademicTermInput
            {
                CurrentTermId = academicSettings.CurrentTermId
            };
        }
    }
}
