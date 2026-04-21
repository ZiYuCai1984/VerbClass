using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class CourseManagerMainMenuItem : MainMenuItem, ISingletonDependency
{
    public override string Href => Routes.Academic_CourseDefinitions;

    public override string Title => "Course Definitions";

    public override int Priority => -90;

    public override Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        return Task.FromResult(context.CurrentUser?.IsAuthenticated ?? false);
    }
}
