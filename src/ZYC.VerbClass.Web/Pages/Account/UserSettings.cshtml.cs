using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Content;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Account;

[Authorize(VerbClassPermissions.UserSettings.Access)]
public class UserSettingsModel : VerbClassPageModel
{
    public UserSettingsModel(
        ILifetimeScope lifetimeScope,
        IUserSettingsAppService userSettingsAppService,
        SignInManager<AbpIdentityUser> signInManager,
        IPermissionChecker permissionChecker) : base(lifetimeScope)
    {
        UserSettingsAppService = userSettingsAppService;
        SignInManager = signInManager;
        PermissionChecker = permissionChecker;
    }

    protected override string PageTitle => "User Settings";

    [BindProperty] public UpdateUserSettingsInput Input { get; set; } = new();

    public string DisplayName { get; private set; } = "User";

    public string UserNameDisplay { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public bool EmailConfirmed { get; private set; }

    public bool HasCustomAvatar { get; private set; }

    public string AvatarVersion { get; private set; } = "default";

    public string[] Roles { get; private set; } = [];

    public bool CanUpdate { get; private set; }

    public IReadOnlyList<SelectListItem> GenderOptions { get; } = BuildGenderOptions();

    private IUserSettingsAppService UserSettingsAppService { get; }

    private SignInManager<AbpIdentityUser> SignInManager { get; }

    private IPermissionChecker PermissionChecker { get; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        CanUpdate = await CanUpdateAsync();
        ApplyPageState(await UserSettingsAppService.GetAsync(), populateInput: true);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        CanUpdate = await CanUpdateAsync();
        if (!CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        ApplyPageState(await UserSettingsAppService.GetAsync(), populateInput: false);

        NormalizeInput();
        ModelState.Clear();
        TryValidateModel(Input, nameof(Input));

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await UserSettingsAppService.SaveAsync(Input);
            await RefreshSignInAsync();

            ToastInfo("Your settings were saved.", "User settings");
            return RedirectToPage();
        }
        catch (AbpValidationException ex)
        {
            ModelStateValidationSupport.ApplyAbpValidationException(
                ModelState,
                ex,
                "User settings could not be saved.",
                nameof(Input)
            );
            return Page();
        }
        catch (UserFriendlyException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile? avatar)
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        CanUpdate = await CanUpdateAsync();
        if (!CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        if (avatar is null || avatar.Length == 0)
        {
            ToastError("Please choose an image file first.", "Upload failed");
            return RedirectToPage();
        }

        try
        {
            await using var stream = avatar.OpenReadStream();
            await UserSettingsAppService.UploadAvatarAsync(
                new RemoteStreamContent(stream, avatar.FileName, avatar.ContentType)
            );
            ToastInfo("Avatar uploaded successfully.", "User settings");
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Upload failed");
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAvatarAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        CanUpdate = await CanUpdateAsync();
        if (!CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        try
        {
            await UserSettingsAppService.RemoveAvatarAsync();
            ToastWarn("Avatar removed.", "User settings");
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Remove failed");
        }

        return RedirectToPage();
    }

    private Task<bool> CanUpdateAsync()
    {
        return PermissionChecker.IsGrantedAsync(VerbClassPermissions.UserSettings.Update);
    }

    private void ApplyPageState(UserSettingsDto settings, bool populateInput)
    {
        DisplayName = settings.DisplayName;
        UserNameDisplay = settings.UserNameDisplay;
        IsActive = settings.IsActive;
        EmailConfirmed = settings.EmailConfirmed;
        HasCustomAvatar = settings.HasCustomAvatar;
        AvatarVersion = settings.AvatarVersion;
        Roles = settings.Roles;

        if (populateInput)
        {
            Input = settings.Input;
        }
    }

    private async Task RefreshSignInAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return;
        }

        var user = await SignInManager.UserManager.FindByIdAsync(CurrentUser.Id.Value.ToString());
        if (user is not null)
        {
            await SignInManager.RefreshSignInAsync(user);
        }
    }

    private void NormalizeInput()
    {
        Input.UserName = Input.UserName.Trim();
        
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
#pragma warning disable CS8601 // Possible null reference assignment.
        //!WARNING For cases about admin's subname is null.
        Input.Surname = Input.Surname?.Trim();
#pragma warning restore CS8601 // Possible null reference assignment.


        Input.Name = Input.Name.Trim();
        Input.Email = Input.Email.Trim();
        Input.PhoneNumber = NormalizeOptional(Input.PhoneNumber);
        Input.SurnameKanji = NormalizeOptional(Input.SurnameKanji);
        Input.NameKanji = NormalizeOptional(Input.NameKanji);
        Input.SurnameKana = NormalizeOptional(Input.SurnameKana);
        Input.NameKana = NormalizeOptional(Input.NameKana);
        Input.SurnameRomanized = NormalizeOptional(Input.SurnameRomanized);
        Input.NameRomanized = NormalizeOptional(Input.NameRomanized);
        Input.BloodType = NormalizeOptional(Input.BloodType);
        Input.Nationality = NormalizeOptional(Input.Nationality);
        Input.Country = NormalizeOptional(Input.Country);
        Input.Prefecture = NormalizeOptional(Input.Prefecture);
        Input.City = NormalizeOptional(Input.City);
        Input.Street = NormalizeOptional(Input.Street);
        Input.PostalCode = NormalizeOptional(Input.PostalCode);
    }

    private static IReadOnlyList<SelectListItem> BuildGenderOptions()
    {
        var options = new List<SelectListItem>
        {
            new("Not set", string.Empty)
        };

        foreach (var gender in Enum.GetValues<Gender>())
        {
            options.Add(new SelectListItem(
                gender.ToString(),
                Convert.ToInt32(gender, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
            ));
        }

        return options;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
