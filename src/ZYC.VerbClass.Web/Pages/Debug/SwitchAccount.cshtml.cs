using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ZYC.VerbClass.Web.Pages.Debug;

[AllowAnonymous]
public class SwitchAccountModel : VerbClassPageModel
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ILoginTenantService _loginTenantService;
    private readonly ILoginUserService _loginUserService;
    private readonly SignInManager<AbpIdentityUser> _signInManager;

    public SwitchAccountModel(
        ILifetimeScope lifetimeScope,
        ILoginTenantService loginTenantService,
        ILoginUserService loginUserService,
        SignInManager<AbpIdentityUser> signInManager,
        ICurrentTenant currentTenant) : base(lifetimeScope)
    {
        _loginTenantService = loginTenantService;
        _loginUserService = loginUserService;
        _signInManager = signInManager;
        _currentTenant = currentTenant;
    }

    [BindProperty(SupportsGet = true)] public string? AccountId { get; set; }

    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var loginUrl = Routes.Account_Login;

        var option = DebugQuickLoginOptions.FindById(AccountId);
        if (option is null)
        {
            ToastError("Debug account was not found.", "Switch account failed");
            return RedirectToPage(loginUrl, new { ReturnUrl = GetSafeReturnUrl() });
        }

        var tenant = await _loginTenantService.ResolveTenantAsync(DebugQuickLoginOptions.LockedTenantName);
        if (!tenant.IsValid)
        {
            ToastError("Debug tenant was not found.", "Switch account failed");
            return RedirectToPage(loginUrl, new { ReturnUrl = GetSafeReturnUrl() });
        }

        await _signInManager.SignOutAsync();

        _loginTenantService.SetTenantCookie(Response, tenant.Name);

        SignInResult result;
        string? loginName;
        using (_currentTenant.Change(tenant.Id))
        {
            loginName = await _loginUserService.ResolveLoginNameAsync(option.UserNameOrEmail);
            if (loginName is null)
            {
                ToastError("Debug account login name could not be resolved.", "Switch account failed");
                return RedirectToPage(loginUrl, new { ReturnUrl = GetSafeReturnUrl() });
            }

            result = await _signInManager.PasswordSignInAsync(
                loginName,
                option.Password,
                true,
                true
            );
        }

        if (result.Succeeded)
        {
            ToastInfo($"Signed in as {loginName}.", "Account switched");
            return LocalRedirect(GetSafeReturnUrl());
        }

        if (result.IsLockedOut)
        {
            ToastError("This debug account is locked.", "Switch account failed");
            return RedirectToPage(loginUrl, new { ReturnUrl = GetSafeReturnUrl() });
        }

        //if (result.RequiresTwoFactor)
        //{
        //    ToastError("Two-factor authentication is not supported on this debug page.", "Switch account failed");
        //    return RedirectToPage(loginUrl, new { ReturnUrl = GetSafeReturnUrl() });
        //}

        ToastError("Debug account sign-in failed.", "Switch account failed");
        return RedirectToPage(loginUrl, new { ReturnUrl = GetSafeReturnUrl() });
    }

    private string GetSafeReturnUrl()
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
            Url.IsLocalUrl(ReturnUrl) &&
            !ReturnUrl.StartsWith(Routes.Debug_Switch, StringComparison.OrdinalIgnoreCase))
        {
            return ReturnUrl;
        }

        return Url.Page("/Index")!;
    }
}