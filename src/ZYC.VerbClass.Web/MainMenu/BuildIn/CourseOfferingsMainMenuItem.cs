using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.MainMenu.BuildIn;

public class CourseOfferingsMainMenuItem : MainMenuItem, ISingletonDependency
{
    public override string Href => Routes.Academic_CourseOfferings;

    public override string Title => "Course Offerings";

    public override int Priority => -89;

    public override Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        return Task.FromResult(context.CurrentUser?.IsAuthenticated ?? false);
    }
}
