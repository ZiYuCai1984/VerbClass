using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions.MainMenu;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class SwitchAccountMainMenuItem : MainMenuItem, ISingletonDependency
{
    public override IMainMenuItem[] SubItems { get; }

    public override string Title => "Switch Account(Debug)";

    public override bool Localization => false;

    public SwitchAccountMainMenuItem()
    {
        SubItems = DebugQuickLoginOptions.All
            .Select(
                (option, index) => (IMainMenuItem)new MainMenuItem(
                    $"{option.Title} ({option.UserNameOrEmail})",
                    DebugQuickLoginOptions.BuildSwitchAccountHref(option.Id),
                    [],
                    priority: index,
                    localization: false
                )
            )
            .ToArray();
    }

    public override async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        await Task.CompletedTask;
        return true;
    }
}
