using Microsoft.AspNetCore.Authorization;

namespace ZYC.VerbClass.Web.Pages.Account;

[AllowAnonymous]
public class AccessDeniedModel : VerbClassPageModel
{
    public AccessDeniedModel(ILifetimeScope lifetimeScope) : base(lifetimeScope)
    {
    }

    protected override string PageTitle => "Access denied";
}