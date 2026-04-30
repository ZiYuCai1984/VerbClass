using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;
using ZYC.VerbClass.Web.Core;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic;

[Authorize]
public class TimetableManagerModel : VerbClassPageModel
{
    public TimetableManagerModel(
        ILifetimeScope lifetimeScope,
        TimetableContentBuilder contentBuilder,
        IAcademicSettingsAppService academicSettingsAppService) : base(lifetimeScope)
    {
        ContentBuilder = contentBuilder;
        AcademicSettingsAppService = academicSettingsAppService;
    }

    private TimetableContentBuilder ContentBuilder { get; }

    private IAcademicSettingsAppService AcademicSettingsAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? TermId { get; set; }

    public TimetableContentModel State { get; private set; } = new();

    protected override string PageTitle => "Timetable Manager";

    public async Task OnGetAsync()
    {
        State = await ContentBuilder.BuildAsync(await ResolveInitialTermIdAsync());
    }

    public async Task<PartialViewResult> OnGetTermAsync(Guid? termId = null)
    {
        SetReplaceUrl(nameof(TermId), termId);
        return await BuildContentPartialAsync(termId);
    }

    private async Task<PartialViewResult> BuildContentPartialAsync(Guid? activeTermId = null)
    {
        return Partial(
            "~/Pages/Academic/TimetableManagerPartials/_TimetableContent.cshtml",
            await ContentBuilder.BuildAsync(activeTermId)
        );
    }

    private async Task<Guid?> ResolveInitialTermIdAsync()
    {
        return TermId ?? await AcademicSettingsAppService.GetCurrentTermIdAsync();
    }
}
