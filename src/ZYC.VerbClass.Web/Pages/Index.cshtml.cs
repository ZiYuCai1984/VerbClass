using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

namespace ZYC.VerbClass.Web.Pages;

[Authorize]
public class IndexModel : VerbClassPageModel
{
    public IndexModel(
        ILifetimeScope lifetimeScope,
        IAcademicSettingsAppService academicSettingsAppService,
        TimetableContentBuilder timetableContentBuilder) : base(lifetimeScope)
    {
        AcademicSettingsAppService = academicSettingsAppService;
        TimetableContentBuilder = timetableContentBuilder;
    }

    private IAcademicSettingsAppService AcademicSettingsAppService { get; }

    private TimetableContentBuilder TimetableContentBuilder { get; }

    [BindProperty(SupportsGet = true)] public Guid? TermId { get; set; }

    public TimetableContentModel TimetableState { get; private set; } = new();

    protected override string PageTitle => "Home";

    public async Task OnGetAsync()
    {
        var termId = TermId ?? await AcademicSettingsAppService.GetCurrentTermIdAsync();
        TimetableState = await TimetableContentBuilder.BuildAsync(termId);
    }

    public async Task<PartialViewResult> OnGetTermAsync(Guid? termId = null)
    {
        SetReplaceUrl(nameof(TermId), termId);

        return Partial(
            "~/Pages/Academic/TimetableManagerPartials/_TimetableContent.cshtml",
            await TimetableContentBuilder.BuildAsync(termId));
    }
}