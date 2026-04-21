using Microsoft.AspNetCore.Authorization;

namespace ZYC.VerbClass.Web.Pages;

[Authorize]
public class IndexModel : VerbClassPageModel
{
    public IndexModel(ILifetimeScope lifetimeScope) : base(lifetimeScope)
    {
    }
}