using ZYC.VerbClass.Web.Abstractions.AccountMenu;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.Core.AccountMenu;

public class AccountMenuItemsProvider : MainMenuItemsProvider, IAccountMenuItem
{
    public AccountMenuItemsProvider(ILifetimeScope lifetimeScope) : base(lifetimeScope)
    {
    }
}