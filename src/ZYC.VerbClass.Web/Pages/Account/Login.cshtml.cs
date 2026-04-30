using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Domain;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ZYC.VerbClass.Web.Pages.Account;

[AllowAnonymous]
public partial class LoginModel : VerbClassPageModel
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ILoginTenantService _loginTenantService;
    private readonly ILoginUserService _loginUserService;
    private readonly SignInManager<AbpIdentityUser> _signInManager;

    public LoginModel(
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

    protected override string PageTitle => "Sign in";

    [BindProperty] public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public string CurrentTenantDisplayName { get; private set; } = "Host";

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        ApplyLockedTenant();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ApplyLockedTenant();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        return await SignInAsync(Input.UserNameOrEmail, Input.Password, Input.RememberMe);
    }

    private async Task<IActionResult> SignInAsync(string userNameOrEmail, string password, bool rememberMe)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl());
        }

        var tenant = await _loginTenantService.ResolveTenantAsync(VerbClassConsts.SakuradaUniversityDemoTenantName);
        if (!tenant.IsValid)
        {
            ApplyLockedTenant();
            ModelState.AddModelError(nameof(Input.TenantName), "Tenant was not found.");
            return Page();
        }

        Input.TenantName = tenant.Name;
        _loginTenantService.SetTenantCookie(Response, tenant.Name);
        SetCurrentTenantDisplayName(tenant.Name);

        SignInResult result;
        string? loginName;
        using (_currentTenant.Change(tenant.Id))
        {
            loginName = await _loginUserService.ResolveLoginNameAsync(userNameOrEmail);
            if (loginName is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid tenant, username, email, or password.");
                return Page();
            }

            result = await _signInManager.PasswordSignInAsync(
                loginName,
                password,
                rememberMe,
                true
            );
        }

        if (result.Succeeded)
        {
            ToastInfo($"Signed in as {loginName}.", "Welcome");
            return LocalRedirect(GetSafeReturnUrl());
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked.");
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            ModelState.AddModelError(string.Empty, "Two-factor authentication is not supported on this page.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid tenant, username, email, or password.");
        return Page();
    }

    private string GetSafeReturnUrl()
    {
        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? ReturnUrl
            : Url.Page("/Index")!;
    }

    private void SetCurrentTenantDisplayName(string? tenantName)
    {
        CurrentTenantDisplayName = _loginTenantService.GetTenantDisplayName(tenantName);
    }

    private void ApplyLockedTenant()
    {
        Input.TenantName = VerbClassConsts.SakuradaUniversityDemoTenantName;
        ModelState.Remove($"{nameof(Input)}.{nameof(InputModel.TenantName)}");
        SetCurrentTenantDisplayName(VerbClassConsts.SakuradaUniversityDemoTenantName);
    }
}
