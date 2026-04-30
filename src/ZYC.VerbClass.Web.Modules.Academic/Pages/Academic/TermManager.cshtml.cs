using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;
using ZYC.VerbClass.Web.Core;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic;

[Authorize]
public class TermManagerModel : VerbClassPageModel
{
    public TermManagerModel(
        ILifetimeScope lifetimeScope,
        IAcademicTermAppService academicTermAppService,
        IAcademicTimeTemplateAppService academicTimeTemplateAppService) : base(lifetimeScope)
    {
        AcademicTermAppService = academicTermAppService;
        AcademicTimeTemplateAppService = academicTimeTemplateAppService;
    }

    private IAcademicTermAppService AcademicTermAppService { get; }

    private IAcademicTimeTemplateAppService AcademicTimeTemplateAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? TermId { get; set; }

    public TermManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Term Manager";

    private static string RouteKey => nameof(TermId);

    public async Task OnGetAsync()
    {
        State = await BuildContentAsync(TermId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid termId)
    {
        return Partial(
            "~/Pages/Academic/TermManagerPartials/_TermInfo.cshtml",
            await BuildInfoModelAsync(termId));
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? termId = null)
    {
        SetReplaceUrl(RouteKey, termId);
        return await BuildContentPartialAsync(termId);
    }

    public async Task<IActionResult> OnGetCreateAsync(Guid? returnTermId = null)
    {
        try
        {
            var editor = await BuildCreateEditorModelAsync(returnTermId);
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(returnTermId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Create unavailable");
            SetReplaceUrl(RouteKey, returnTermId);
            return await BuildContentPartialAsync(returnTermId);
        }
    }

    public async Task<IActionResult> OnGetEditAsync(Guid termId)
    {
        try
        {
            var editor = await BuildEditEditorModelAsync(termId);
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId);
        }
    }

    public async Task<PartialViewResult> OnPostCreateAsync(TermEditorInput input)
    {
        var editor = await BuildEditorModelAsync(input, true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(editor.ReturnTermId, editor);
        }

        try
        {
            var term = await AcademicTermAppService.CreateAsync(ToCreateInput(editor));
            ToastInfo($"Term {term.Name} was created.", "Term created");
            SetReplaceUrl(RouteKey, term.Id);
            return await BuildContentPartialAsync(term.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Term operation failed.");
            editor.TemplateOptions = await BuildTemplateOptionsAsync();
            editor.Periods = await BuildPreviewPeriodsAsync(editor.TimeTemplateId);
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(editor.ReturnTermId, editor);
        }
    }

    public async Task<PartialViewResult> OnPostUpdateAsync(Guid termId, TermEditorInput input)
    {
        var editor = await BuildEditorModelAsync(input, false, termId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId, editor);
        }

        try
        {
            var term = await AcademicTermAppService.UpdateAsync(termId, ToUpdateInput(editor));
            ToastInfo($"Term {term.Name} was updated.", "Term saved");
            SetReplaceUrl(RouteKey, term.Id);
            return await BuildContentPartialAsync(term.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Term operation failed.");
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId);
        }
    }

    public async Task<PartialViewResult> OnPostLockAsync(Guid termId)
    {
        try
        {
            var term = await AcademicTermAppService.LockAsync(termId);
            ToastInfo($"Term {term.Name} was locked.", "Term locked");
            SetReplaceUrl(RouteKey, term.Id);
            return await BuildContentPartialAsync(term.Id);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Lock failed");
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId);
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid termId)
    {
        try
        {
            var term = await AcademicTermAppService.DeleteAsync(termId);
            ToastWarn($"Term {term.Name} was deleted.", "Term deleted");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, termId);
            return await BuildContentPartialAsync(termId);
        }
    }

    private async Task<PartialViewResult> BuildContentPartialAsync(
        Guid? activeTermId = null,
        TermEditorModel? editor = null)
    {
        return Partial("~/Pages/Academic/TermManagerPartials/_TermManagerContent.cshtml",
            await BuildContentAsync(activeTermId, editor));
    }

    private async Task<TermManagerContentModel> BuildContentAsync(
        Guid? activeTermId = null,
        TermEditorModel? editor = null)
    {
        var terms = (await AcademicTermAppService.GetListAsync())
            .Select(term => new TermListItem(
                term.Id,
                term.AcademicYear,
                term.Code,
                term.Name,
                term.StartDate,
                term.EndDate,
                term.IsLocked,
                term.PeriodCount))
            .ToArray();

        if (activeTermId.HasValue && terms.All(term => term.Id != activeTermId.Value))
        {
            activeTermId = null;
        }

        TermInfoModel? selectedTerm = null;
        if (editor is null && activeTermId.HasValue)
        {
            selectedTerm = await BuildInfoModelAsync(activeTermId.Value);
        }

        return new TermManagerContentModel
        {
            Terms = terms,
            ActiveTermId = activeTermId,
            SelectedTerm = selectedTerm,
            Editor = editor
        };
    }

    private async Task<TermInfoModel> BuildInfoModelAsync(Guid termId)
    {
        var term = await AcademicTermAppService.GetAsync(termId);

        return new TermInfoModel
        {
            Id = term.Id,
            AcademicYear = term.AcademicYear,
            Code = term.Code,
            Name = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            IsLocked = term.IsLocked,
            Periods = term.Periods
                .Select(period => new PeriodRowModel(
                    period.PeriodNo,
                    period.Label,
                    period.StartTime.ToString("HH:mm"),
                    period.EndTime.ToString("HH:mm")))
                .ToArray()
        };
    }

    private async Task<TermEditorModel> BuildCreateEditorModelAsync(Guid? returnTermId = null)
    {
        var templateOptions = await BuildTemplateOptionsAsync();
        if (templateOptions.Length == 0)
        {
            throw new UserFriendlyException("Create a time template before creating a term.");
        }

        return new TermEditorModel
        {
            IsCreate = true,
            ReturnTermId = returnTermId,
            AcademicYear = DateTime.Today.Year,
            TemplateOptions = templateOptions,
            Periods = []
        };
    }

    private async Task<TermEditorModel> BuildEditEditorModelAsync(Guid termId)
    {
        var term = await AcademicTermAppService.GetAsync(termId);
        if (term.IsLocked)
        {
            throw new UserFriendlyException("Locked terms cannot be modified.");
        }

        return new TermEditorModel
        {
            TermId = term.Id,
            ReturnTermId = term.Id,
            AcademicYear = term.AcademicYear,
            Code = term.Code,
            Name = term.Name,
            StartDate = term.StartDate,
            EndDate = term.EndDate,
            IsLocked = term.IsLocked,
            Periods = term.Periods
                .Select(period => new PeriodRowModel(
                    period.PeriodNo,
                    period.Label,
                    period.StartTime.ToString("HH:mm"),
                    period.EndTime.ToString("HH:mm")))
                .ToArray()
        };
    }

    private async Task<TermEditorModel> BuildEditorModelAsync(
        TermEditorInput input,
        bool isCreate,
        Guid? termId = null)
    {
        var periods = isCreate
            ? await BuildPreviewPeriodsAsync(input.TimeTemplateId)
            : termId.HasValue
                ? (await BuildInfoModelAsync(termId.Value)).Periods
                : [];

        return new TermEditorModel
        {
            IsCreate = isCreate,
            TermId = termId,
            ReturnTermId = input.ReturnTermId,
            AcademicYear = input.AcademicYear,
            Code = input.Code,
            Name = input.Name,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            TimeTemplateId = input.TimeTemplateId,
            TemplateOptions = isCreate ? await BuildTemplateOptionsAsync() : [],
            Periods = periods
        };
    }

    private async Task<TemplateOptionModel[]> BuildTemplateOptionsAsync()
    {
        return (await AcademicTimeTemplateAppService.GetOptionsAsync())
            .Select(option => new TemplateOptionModel(
                option.Id,
                option.Code,
                option.Name,
                option.Periods
                    .Select(period => new PeriodRowModel(
                        period.PeriodNo,
                        period.Label,
                        period.StartTime.ToString("HH:mm"),
                        period.EndTime.ToString("HH:mm")))
                    .ToArray()))
            .ToArray();
    }

    private async Task<PeriodRowModel[]> BuildPreviewPeriodsAsync(Guid? templateId)
    {
        if (!templateId.HasValue || templateId.Value == Guid.Empty)
        {
            return [];
        }

        var options = await BuildTemplateOptionsAsync();
        return options
            .FirstOrDefault(option => option.Id == templateId.Value)?
            .Periods ?? [];
    }

    private static CreateAcademicTermInput ToCreateInput(TermEditorModel editor)
    {
        return new CreateAcademicTermInput
        {
            AcademicYear = editor.AcademicYear!.Value,
            Code = editor.Code,
            Name = editor.Name,
            TimeTemplateId = editor.TimeTemplateId,
            StartDate = editor.StartDate!.Value,
            EndDate = editor.EndDate!.Value
        };
    }

    private static UpdateAcademicTermInput ToUpdateInput(TermEditorModel editor)
    {
        return new UpdateAcademicTermInput
        {
            AcademicYear = editor.AcademicYear!.Value,
            Code = editor.Code,
            Name = editor.Name,
            StartDate = editor.StartDate!.Value,
            EndDate = editor.EndDate!.Value
        };
    }

    private static void NormalizeEditor(TermEditorModel editor)
    {
        editor.Code = editor.Code.Trim();
        editor.Name = editor.Name.Trim();
    }

    private static void ValidateEditor(TermEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);

        if (!editor.StartDate.HasValue)
        {
            editor.AddFieldError(nameof(editor.StartDate), "Start date is required.");
        }

        if (!editor.EndDate.HasValue)
        {
            editor.AddFieldError(nameof(editor.EndDate), "End date is required.");
        }

        if (editor.StartDate.HasValue &&
            editor.EndDate.HasValue &&
            editor.StartDate.Value > editor.EndDate.Value)
        {
            editor.AddFieldError(nameof(editor.EndDate), "End date must be later than start date.");
        }

        if (editor.IsCreate && !editor.TimeTemplateId.HasValue)
        {
            editor.AddFieldError(nameof(editor.TimeTemplateId), "Template is required.");
        }
    }
}