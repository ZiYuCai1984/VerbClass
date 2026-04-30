using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;
using ZYC.VerbClass.Web.Core;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic;

[Authorize]
public class AcademicTimeTemplateManagerModel : VerbClassPageModel
{
    public AcademicTimeTemplateManagerModel(
        ILifetimeScope lifetimeScope,
        IAcademicTimeTemplateAppService academicTimeTemplateAppService) : base(lifetimeScope)
    {
        AcademicTimeTemplateAppService = academicTimeTemplateAppService;
    }

    private IAcademicTimeTemplateAppService AcademicTimeTemplateAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? TemplateId { get; set; }

    public TimeTemplateManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Time Templates";

    private static string RouteKey => nameof(TemplateId);

    public async Task OnGetAsync()
    {
        State = await BuildContentAsync(TemplateId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid templateId)
    {
        return Partial("~/Pages/Academic/TimeTemplateManagerPartials/_TimeTemplateInfo.cshtml", await BuildInfoModelAsync(templateId));
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? templateId = null)
    {
        SetReplaceUrl(RouteKey, templateId);
        return await BuildContentPartialAsync(templateId);
    }

    public async Task<PartialViewResult> OnGetCreateAsync(Guid? returnTemplateId = null)
    {
        SetReplaceUrl(RouteKey);
        return await BuildContentPartialAsync(
            returnTemplateId,
            BuildCreateEditorModel(returnTemplateId)
        );
    }

    public async Task<IActionResult> OnGetEditAsync(Guid templateId)
    {
        try
        {
            var editor = await BuildEditEditorModelAsync(templateId);
            SetReplaceUrl(RouteKey, templateId);
            return await BuildContentPartialAsync(templateId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
    }

    public async Task<IActionResult> OnGetCopyAsync(Guid templateId)
    {
        try
        {
            var editor = await BuildCopyEditorModelAsync(templateId);
            SetReplaceUrl(RouteKey, templateId);
            return await BuildContentPartialAsync(templateId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Copy failed");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostCreateAsync(TimeTemplateEditorInput input)
    {
        var editor = BuildEditorModel(input, isCreate: true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(editor.ReturnTemplateId, editor);
        }

        try
        {
            var template = await AcademicTimeTemplateAppService.CreateAsync(ToCreateInput(editor));

            ToastInfo($"Template {template.Name} was created.", "Template created");
            SetReplaceUrl(RouteKey, template.Id);
            return await BuildContentPartialAsync(template.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Template operation failed.");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(editor.ReturnTemplateId, editor);
        }
    }

    public async Task<PartialViewResult> OnPostUpdateAsync(Guid templateId, TimeTemplateEditorInput input)
    {
        var editor = BuildEditorModel(input, isCreate: false, templateId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, templateId);
            return await BuildContentPartialAsync(templateId, editor);
        }

        try
        {
            var template = await AcademicTimeTemplateAppService.UpdateAsync(templateId, ToUpdateInput(editor));

            ToastInfo($"Template {template.Name} was updated.", "Template saved");
            SetReplaceUrl(RouteKey, template.Id);
            return await BuildContentPartialAsync(template.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Template operation failed.");
            SetReplaceUrl(RouteKey, templateId);
            return await BuildContentPartialAsync(templateId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid templateId)
    {
        try
        {
            var template = await AcademicTimeTemplateAppService.DeleteAsync(templateId);
            ToastWarn($"Template {template.Name} was deleted.", "Template deleted");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, templateId);
            return await BuildContentPartialAsync(templateId);
        }
    }

    private async Task<PartialViewResult> BuildContentPartialAsync(
        Guid? activeTemplateId = null,
        TimeTemplateEditorModel? editor = null)
    {
        return Partial("~/Pages/Academic/TimeTemplateManagerPartials/_TimeTemplateManagerContent.cshtml", await BuildContentAsync(activeTemplateId, editor));
    }

    private async Task<TimeTemplateManagerContentModel> BuildContentAsync(
        Guid? activeTemplateId = null,
        TimeTemplateEditorModel? editor = null)
    {
        var templates = (await AcademicTimeTemplateAppService.GetListAsync())
            .Select(template => new TimeTemplateListItem(
                template.Id,
                template.Code,
                template.Name,
                template.PeriodCount))
            .ToArray();

        if (activeTemplateId.HasValue && templates.All(template => template.Id != activeTemplateId.Value))
        {
            activeTemplateId = null;
        }

        TimeTemplateInfoModel? selectedTemplate = null;
        if (editor is null && activeTemplateId.HasValue)
        {
            selectedTemplate = await BuildInfoModelAsync(activeTemplateId.Value);
        }

        return new TimeTemplateManagerContentModel
        {
            Templates = templates,
            ActiveTemplateId = activeTemplateId,
            SelectedTemplate = selectedTemplate,
            Editor = editor
        };
    }

    private async Task<TimeTemplateInfoModel> BuildInfoModelAsync(Guid templateId)
    {
        var template = await AcademicTimeTemplateAppService.GetAsync(templateId);

        return new TimeTemplateInfoModel
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Periods = MapPeriods(template.Periods)
        };
    }

    private static TimeTemplateEditorModel BuildCreateEditorModel(Guid? returnTemplateId = null)
    {
        return new TimeTemplateEditorModel
        {
            IsCreate = true,
            ReturnTemplateId = returnTemplateId,
            Periods =
            [
                new TimeTemplatePeriodEditorItem()
            ]
        };
    }

    private async Task<TimeTemplateEditorModel> BuildEditEditorModelAsync(Guid templateId)
    {
        var template = await AcademicTimeTemplateAppService.GetAsync(templateId);

        return new TimeTemplateEditorModel
        {
            TemplateId = template.Id,
            ReturnTemplateId = template.Id,
            Code = template.Code,
            Name = template.Name,
            Periods = template.Periods
                .Select(period => new TimeTemplatePeriodEditorItem
                {
                    PeriodNo = period.PeriodNo,
                    Label = period.Label,
                    StartTime = period.StartTime.ToString("HH:mm"),
                    EndTime = period.EndTime.ToString("HH:mm")
                })
                .ToList()
        };
    }

    private async Task<TimeTemplateEditorModel> BuildCopyEditorModelAsync(Guid templateId)
    {
        var editor = await BuildEditEditorModelAsync(templateId);
        editor.IsCreate = true;
        editor.IsCopy = true;
        editor.TemplateId = null;
        editor.Code = $"{editor.Code}-COPY";
        editor.Name = $"{editor.Name} Copy";
        return editor;
    }

    private static TimeTemplateEditorModel BuildEditorModel(
        TimeTemplateEditorInput input,
        bool isCreate,
        Guid? templateId = null)
    {
        return new TimeTemplateEditorModel
        {
            IsCreate = isCreate,
            TemplateId = templateId,
            ReturnTemplateId = input.ReturnTemplateId,
            Code = input.Code,
            Name = input.Name,
            Periods = input.Periods ?? []
        };
    }

    private static CreateAcademicTimeTemplateInput ToCreateInput(TimeTemplateEditorModel editor)
    {
        return new CreateAcademicTimeTemplateInput
        {
            Code = editor.Code,
            Name = editor.Name,
            Periods = editor.Periods
                .Select(period => new AcademicPeriodDefinitionDto
                {
                    PeriodNo = period.PeriodNo,
                    Label = period.Label,
                    StartTime = ParseTime(period.StartTime),
                    EndTime = ParseTime(period.EndTime)
                })
                .ToArray()
        };
    }

    private static UpdateAcademicTimeTemplateInput ToUpdateInput(TimeTemplateEditorModel editor)
    {
        return new UpdateAcademicTimeTemplateInput
        {
            Code = editor.Code,
            Name = editor.Name,
            Periods = editor.Periods
                .Select(period => new AcademicPeriodDefinitionDto
                {
                    PeriodNo = period.PeriodNo,
                    Label = period.Label,
                    StartTime = ParseTime(period.StartTime),
                    EndTime = ParseTime(period.EndTime)
                })
                .ToArray()
        };
    }

    private static void NormalizeEditor(TimeTemplateEditorModel editor)
    {
        editor.Code = editor.Code.Trim();
        editor.Name = editor.Name.Trim();

        foreach (var period in editor.Periods)
        {
            period.Label = period.Label.Trim();
            period.StartTime = period.StartTime.Trim();
            period.EndTime = period.EndTime.Trim();
        }
    }

    private static void ValidateEditor(TimeTemplateEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);

        if (editor.Periods.Count == 0)
        {
            editor.AddFieldError(nameof(editor.Periods), "At least one period is required.");
            return;
        }

        var parsedPeriods = new List<(int Index, int PeriodNo, TimeOnly StartTime, TimeOnly EndTime)>();

        for (var i = 0; i < editor.Periods.Count; i++)
        {
            var period = editor.Periods[i];
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(period, new ValidationContext(period), validationResults, true);
            var hasFieldErrors = validationResults.Count > 0;

            foreach (var validationResult in validationResults)
            {
                var members = validationResult.MemberNames.Any()
                    ? validationResult.MemberNames.Select(member => $"Periods[{i}].{member}")
                    : [$"Periods[{i}]"];

                foreach (var member in members)
                {
                    editor.AddFieldError(member, validationResult.ErrorMessage ?? "Period is invalid.");
                }
            }

            var hasTimeErrors = false;

            if (!TryParseTime(period.StartTime, out var startTime))
            {
                editor.AddFieldError($"Periods[{i}].{nameof(TimeTemplatePeriodEditorItem.StartTime)}",
                    "Use HH:mm.");
                hasTimeErrors = true;
            }

            if (!TryParseTime(period.EndTime, out var endTime))
            {
                editor.AddFieldError($"Periods[{i}].{nameof(TimeTemplatePeriodEditorItem.EndTime)}",
                    "Use HH:mm.");
                hasTimeErrors = true;
            }

            if (hasTimeErrors)
            {
                continue;
            }

            if (hasFieldErrors)
            {
                continue;
            }

            if (startTime >= endTime)
            {
                editor.AddFieldError($"Periods[{i}].{nameof(TimeTemplatePeriodEditorItem.EndTime)}",
                    "End time must be later than start time.");
                continue;
            }

            parsedPeriods.Add((i, period.PeriodNo, startTime, endTime));
        }

        var duplicateGroups = parsedPeriods
            .GroupBy(x => x.PeriodNo)
            .Where(x => x.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            foreach (var item in group)
            {
                editor.AddFieldError($"Periods[{item.Index}].{nameof(TimeTemplatePeriodEditorItem.PeriodNo)}",
                    "Period numbers must be unique.");
            }
        }

        foreach (var pair in parsedPeriods
                     .OrderBy(x => x.PeriodNo)
                     .Zip(parsedPeriods.OrderBy(x => x.PeriodNo).Skip(1)))
        {
            var previous = pair.First;
            var current = pair.Second;

            if (current.StartTime < previous.EndTime)
            {
                editor.AddFieldError($"Periods[{current.Index}].{nameof(TimeTemplatePeriodEditorItem.StartTime)}",
                    "Periods must not overlap.");
            }
        }
    }

    private static PeriodRowModel[] MapPeriods(IEnumerable<AcademicPeriodDefinitionDto> periods)
    {
        return periods
            .Select(period => new PeriodRowModel(
                period.PeriodNo,
                period.Label,
                period.StartTime.ToString("HH:mm"),
                period.EndTime.ToString("HH:mm")))
            .ToArray();
    }

    private static bool TryParseTime(string value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(
            value,
            ["HH:mm", "H:mm"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }

    private static TimeOnly ParseTime(string value)
    {
        if (!TryParseTime(value, out var time))
        {
            throw new AbpException($"Invalid time value '{value}'.");
        }

        return time;
    }
}
