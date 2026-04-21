using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class MyCoursesMainMenuItem : MainMenuItem, ISingletonDependency
{
    public override string Href => Routes.Academic_MyCourses;

    public override string Title => "My Courses";

    public override int Priority => -100;

    public override Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        return Task.FromResult(context.CurrentUser?.IsAuthenticated ?? false);
    }
}
