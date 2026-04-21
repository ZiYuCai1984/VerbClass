using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Core.AccountMenu;

namespace ZYC.VerbClass.Web.AccountMenu.BuildIn;

public class LogoutAccountMenuItem : AccountMenuItem, ISingletonDependency
{
    public override string Href => Routes.Account_Logout;

    public override string Title => "Sign Out";
}