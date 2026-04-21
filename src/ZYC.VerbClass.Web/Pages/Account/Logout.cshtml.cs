using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ZYC.VerbClass.Web.Pages.Account;

[Authorize]
public class LogoutModel : VerbClassPageModel
{
    private readonly SignInManager<AbpIdentityUser> _signInManager;

    public LogoutModel(
        SignInManager<AbpIdentityUser> signInManager,
        ILifetimeScope lifetimeScope) : base(lifetimeScope)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        return await SignOutAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return await SignOutAsync();
    }

    private async Task<IActionResult> SignOutAsync()
    {
        await _signInManager.SignOutAsync();
        ToastInfo("You have been signed out.", "Session ended");
        return RedirectToPage(Routes.Account_Login);
    }
}