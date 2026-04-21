using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions.MainMenu;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.Modules.Mock;

public class MockMainMenuItem : MainMenuItem, ISingletonDependency
{
    public override string Href => "/mock";

    public override string Title => "Mock";

    public override IMainMenuItem[] SubItems { get; } = [];
}