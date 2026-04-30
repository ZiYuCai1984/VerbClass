using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions;
using ZYC.VerbClass.Web.Abstractions.MainMenu;
using ZYC.VerbClass.Web.Core.MainMenu;

namespace ZYC.VerbClass.Web.Modules.Academic;

public class AcademicMainMenuItem : MainMenuItem, ISingletonDependency
{
    public override string Href => "";

    public override string Title => "Academic";

    public override bool Localization => false;

    public override IMainMenuItem[] SubItems { get; } =
    [
        new MainMenuItem("Time Templates", Routes.Academic_TimeTemplateManager, [], localization: false),
        new MainMenuItem("Terms", Routes.Academic_TermManager, [], localization: false),
        new MainMenuItem("Course Definitions", Routes.Academic_CourseDefinitionManager, [], localization: false),
        new MainMenuItem("Course Offerings", Routes.Academic_CourseOfferingManager, [], localization: false),
        new MainMenuItem("Course Participants", Routes.Academic_CourseParticipantManager, [], localization: false),
        new MainMenuItem("Timetable", Routes.Academic_TimetableManager, [], localization: false)
    ];

    public override Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        return Task.FromResult(context.CurrentUser?.IsAuthenticated ?? false);
    }
}
