using System.ComponentModel.DataAnnotations;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic;

[Authorize]
public class CourseOfferingManagerModel : VerbClassPageModel
{
    public CourseOfferingManagerModel(
        ILifetimeScope lifetimeScope,
        IAcademicTermAppService academicTermAppService,
        IAcademicSettingsAppService academicSettingsAppService,
        ICourseDefinitionAppService courseDefinitionAppService,
        ICourseOfferingAppService courseOfferingAppService) : base(lifetimeScope)
    {
        AcademicTermAppService = academicTermAppService;
        AcademicSettingsAppService = academicSettingsAppService;
        CourseDefinitionAppService = courseDefinitionAppService;
        CourseOfferingAppService = courseOfferingAppService;
    }

    private IAcademicTermAppService AcademicTermAppService { get; }

    private IAcademicSettingsAppService AcademicSettingsAppService { get; }

    private ICourseDefinitionAppService CourseDefinitionAppService { get; }

    private ICourseOfferingAppService CourseOfferingAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? TermId { get; set; }

    [BindProperty(SupportsGet = true)] public Guid? OfferingId { get; set; }

    public CourseOfferingManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Course Offerings";

    public async Task OnGetAsync()
    {
        State = await BuildContentAsync(await ResolveInitialTermIdAsync(), OfferingId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid offeringId)
    {
        return Partial("~/Pages/Academic/CourseOfferingManagerPartials/_CourseOfferingInfo.cshtml",
            await BuildInfoModelAsync(offeringId));
    }

    public async Task<PartialViewResult> OnGetTermAsync(Guid? termId = null)
    {
        SetCourseOfferingReplaceUrl(termId);
        return await BuildContentPartialAsync(termId);
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? termId = null, Guid? offeringId = null)
    {
        SetCourseOfferingReplaceUrl(termId, offeringId);
        return await BuildContentPartialAsync(termId, offeringId);
    }

    public async Task<IActionResult> OnGetCreateAsync(Guid termId, Guid? returnOfferingId = null)
    {
        try
        {
            var editor = await BuildCreateEditorModelAsync(termId, returnOfferingId);
            SetCourseOfferingReplaceUrl(termId);
            return await BuildContentPartialAsync(termId, returnOfferingId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Create unavailable");
            SetCourseOfferingReplaceUrl(termId, returnOfferingId);
            return await BuildContentPartialAsync(termId, returnOfferingId);
        }
    }

    public async Task<IActionResult> OnGetEditAsync(Guid offeringId)
    {
        try
        {
            var editor = await BuildEditEditorModelAsync(offeringId);
            SetCourseOfferingReplaceUrl(editor.AcademicTermId, offeringId);
            return await BuildContentPartialAsync(editor.AcademicTermId, offeringId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetCourseOfferingReplaceUrl(TermId);
            return await BuildContentPartialAsync(TermId);
        }
    }

    public async Task<PartialViewResult> OnPostCreateAsync(CourseOfferingEditorInput input)
    {
        var editor = await BuildEditorModelAsync(input, true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetCourseOfferingReplaceUrl(editor.AcademicTermId, editor.ReturnOfferingId);
            return await BuildContentPartialAsync(editor.AcademicTermId, editor.ReturnOfferingId, editor);
        }

        try
        {
            var courseOffering = await CourseOfferingAppService.CreateAsync(ToCreateInput(editor));
            ToastInfo($"Offering {courseOffering.OfferingCode} was created.", "Offering created");
            SetCourseOfferingReplaceUrl(courseOffering.AcademicTermId, courseOffering.Id);
            return await BuildContentPartialAsync(courseOffering.AcademicTermId, courseOffering.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Course offering operation failed.");
            SetCourseOfferingReplaceUrl(editor.AcademicTermId, editor.ReturnOfferingId);
            return await BuildContentPartialAsync(editor.AcademicTermId, editor.ReturnOfferingId, editor);
        }
    }

    public async Task<PartialViewResult> OnPostUpdateAsync(
        Guid offeringId,
        CourseOfferingEditorInput input)
    {
        var editor = await BuildEditorModelAsync(input, false, offeringId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetCourseOfferingReplaceUrl(editor.AcademicTermId, offeringId);
            return await BuildContentPartialAsync(editor.AcademicTermId, offeringId, editor);
        }

        try
        {
            var courseOffering = await CourseOfferingAppService.UpdateAsync(offeringId, ToUpdateInput(editor));
            ToastInfo($"Offering {courseOffering.OfferingCode} was updated.", "Offering saved");
            SetCourseOfferingReplaceUrl(courseOffering.AcademicTermId, courseOffering.Id);
            return await BuildContentPartialAsync(courseOffering.AcademicTermId, courseOffering.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Course offering operation failed.");
            SetCourseOfferingReplaceUrl(editor.AcademicTermId, offeringId);
            return await BuildContentPartialAsync(editor.AcademicTermId, offeringId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetCourseOfferingReplaceUrl(editor.AcademicTermId, offeringId);
            return await BuildContentPartialAsync(editor.AcademicTermId, offeringId);
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid offeringId)
    {
        try
        {
            var courseOffering = await CourseOfferingAppService.DeleteAsync(offeringId);
            ToastWarn($"Offering {courseOffering.OfferingCode} was deleted.", "Offering deleted");
            SetCourseOfferingReplaceUrl(courseOffering.AcademicTermId);
            return await BuildContentPartialAsync(courseOffering.AcademicTermId);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetCourseOfferingReplaceUrl(TermId, offeringId);
            return await BuildContentPartialAsync(TermId, offeringId);
        }
    }

    private async Task<PartialViewResult> BuildContentPartialAsync(
        Guid? activeTermId = null,
        Guid? activeOfferingId = null,
        CourseOfferingEditorModel? editor = null)
    {
        return Partial(
            "~/Pages/Academic/CourseOfferingManagerPartials/_CourseOfferingManagerContent.cshtml",
            await BuildContentAsync(activeTermId, activeOfferingId, editor)
        );
    }

    private async Task<CourseOfferingManagerContentModel> BuildContentAsync(
        Guid? activeTermId = null,
        Guid? activeOfferingId = null,
        CourseOfferingEditorModel? editor = null)
    {
        var terms = await BuildTermOptionsAsync();

        if (activeTermId.HasValue && terms.All(term => term.Id != activeTermId.Value))
        {
            activeTermId = null;
            activeOfferingId = null;
        }

        CourseOfferingListItem[] offerings = [];
        if (activeTermId.HasValue)
        {
            offerings = (await CourseOfferingAppService.GetListAsync(activeTermId.Value))
                .Select(offering => new CourseOfferingListItem(
                    offering.Id,
                    offering.AcademicTermId,
                    offering.OfferingCode,
                    offering.CourseCodeSnapshot,
                    offering.CourseNameSnapshot,
                    FormatScheduleSummary(offering.ScheduleSlots)))
                .ToArray();
        }

        if (activeOfferingId.HasValue && offerings.All(offering => offering.Id != activeOfferingId.Value))
        {
            activeOfferingId = null;
        }

        CourseOfferingInfoModel? selectedOffering = null;
        if (editor is null && activeOfferingId.HasValue)
        {
            selectedOffering = await BuildInfoModelAsync(activeOfferingId.Value);
        }

        return new CourseOfferingManagerContentModel
        {
            Terms = terms,
            ActiveTermId = activeTermId,
            Offerings = offerings,
            ActiveOfferingId = activeOfferingId,
            SelectedOffering = selectedOffering,
            Editor = editor
        };
    }

    private async Task<Guid?> ResolveInitialTermIdAsync()
    {
        return TermId ?? await AcademicSettingsAppService.GetCurrentTermIdAsync();
    }

    private async Task<CourseOfferingInfoModel> BuildInfoModelAsync(Guid offeringId)
    {
        var offering = await CourseOfferingAppService.GetAsync(offeringId);

        return new CourseOfferingInfoModel
        {
            Id = offering.Id,
            AcademicTermId = offering.AcademicTermId,
            AcademicTermTitle = FormatTermTitle(
                offering.AcademicYear,
                offering.AcademicTermCode,
                offering.AcademicTermName
            ),
            CourseDefinitionId = offering.CourseDefinitionId,
            OfferingCode = offering.OfferingCode,
            CourseCodeSnapshot = offering.CourseCodeSnapshot,
            CourseNameSnapshot = offering.CourseNameSnapshot,
            ScheduleSlots = MapScheduleRows(offering.ScheduleSlots, offering.TermPeriods)
        };
    }

    private async Task<CourseOfferingEditorModel> BuildCreateEditorModelAsync(
        Guid termId,
        Guid? returnOfferingId = null)
    {
        var courseDefinitionOptions = await BuildCourseDefinitionOptionsAsync();
        if (courseDefinitionOptions.Length == 0)
        {
            throw new UserFriendlyException("Create an active course definition before creating an offering.");
        }

        var term = await AcademicTermAppService.GetAsync(termId);

        return new CourseOfferingEditorModel
        {
            IsCreate = true,
            AcademicTermId = term.Id,
            ReturnTermId = term.Id,
            ReturnOfferingId = returnOfferingId,
            AcademicTermTitle = FormatTermTitle(term.AcademicYear, term.Code, term.Name),
            CourseDefinitionOptions = courseDefinitionOptions,
            TermPeriods = MapTermPeriodOptions(term.Periods),
            WeekdayOptions = BuildWeekdayOptions()
        };
    }

    private async Task<CourseOfferingEditorModel> BuildEditEditorModelAsync(Guid offeringId)
    {
        var offering = await CourseOfferingAppService.GetAsync(offeringId);

        return new CourseOfferingEditorModel
        {
            CourseOfferingId = offering.Id,
            AcademicTermId = offering.AcademicTermId,
            ReturnTermId = offering.AcademicTermId,
            ReturnOfferingId = offering.Id,
            OfferingCode = offering.OfferingCode,
            CourseDefinitionId = offering.CourseDefinitionId,
            CourseCodeSnapshot = offering.CourseCodeSnapshot,
            CourseNameSnapshot = offering.CourseNameSnapshot,
            AcademicTermTitle = FormatTermTitle(
                offering.AcademicYear,
                offering.AcademicTermCode,
                offering.AcademicTermName
            ),
            TermPeriods = MapTermPeriodOptions(offering.TermPeriods),
            WeekdayOptions = BuildWeekdayOptions(),
            ScheduleSlots = offering.ScheduleSlots
                .Select(slot => new CourseOfferingScheduleSlotEditorItem
                {
                    Weekday = slot.Weekday,
                    PeriodNo = slot.PeriodNo
                })
                .ToList()
        };
    }

    private async Task<CourseOfferingEditorModel> BuildEditorModelAsync(
        CourseOfferingEditorInput input,
        bool isCreate,
        Guid? offeringId = null)
    {
        var termPeriods = input.AcademicTermId.HasValue
            ? await BuildTermPeriodOptionsAsync(input.AcademicTermId.Value)
            : [];

        return new CourseOfferingEditorModel
        {
            IsCreate = isCreate,
            CourseOfferingId = offeringId,
            AcademicTermId = input.AcademicTermId,
            CourseDefinitionId = input.CourseDefinitionId,
            OfferingCode = input.OfferingCode,
            ScheduleSlots = input.ScheduleSlots ?? [],
            ReturnTermId = input.ReturnTermId,
            ReturnOfferingId = input.ReturnOfferingId,
            AcademicTermTitle = input.AcademicTermId.HasValue
                ? await BuildTermTitleAsync(input.AcademicTermId.Value)
                : string.Empty,
            CourseDefinitionOptions = isCreate ? await BuildCourseDefinitionOptionsAsync() : [],
            TermPeriods = termPeriods,
            WeekdayOptions = BuildWeekdayOptions()
        };
    }

    private async Task<CourseOfferingTermOptionModel[]> BuildTermOptionsAsync()
    {
        return (await AcademicTermAppService.GetListAsync())
            .Select(term => new CourseOfferingTermOptionModel(
                term.Id,
                term.AcademicYear,
                term.Code,
                term.Name))
            .ToArray();
    }

    private async Task<CourseDefinitionOptionModel[]> BuildCourseDefinitionOptionsAsync()
    {
        return (await CourseDefinitionAppService.GetActiveOptionsAsync())
            .Select(option => new CourseDefinitionOptionModel(
                option.Id,
                option.Code,
                option.Name))
            .ToArray();
    }

    private async Task<CourseOfferingTermPeriodOptionModel[]> BuildTermPeriodOptionsAsync(Guid termId)
    {
        var term = await AcademicTermAppService.GetAsync(termId);
        return MapTermPeriodOptions(term.Periods);
    }

    private async Task<string> BuildTermTitleAsync(Guid termId)
    {
        var term = await AcademicTermAppService.GetAsync(termId);
        return FormatTermTitle(term.AcademicYear, term.Code, term.Name);
    }

    private static CreateCourseOfferingInput ToCreateInput(CourseOfferingEditorModel editor)
    {
        return new CreateCourseOfferingInput
        {
            AcademicTermId = editor.AcademicTermId,
            CourseDefinitionId = editor.CourseDefinitionId,
            OfferingCode = editor.OfferingCode,
            ScheduleSlots = ToScheduleDtos(editor.ScheduleSlots)
        };
    }

    private static UpdateCourseOfferingInput ToUpdateInput(CourseOfferingEditorModel editor)
    {
        return new UpdateCourseOfferingInput
        {
            OfferingCode = editor.OfferingCode,
            ScheduleSlots = ToScheduleDtos(editor.ScheduleSlots)
        };
    }

    private static AcademicScheduleSlotDto[] ToScheduleDtos(
        IEnumerable<CourseOfferingScheduleSlotEditorItem> scheduleSlots)
    {
        return scheduleSlots
            .Select(slot => new AcademicScheduleSlotDto
            {
                Weekday = slot.Weekday!.Value,
                PeriodNo = slot.PeriodNo!.Value
            })
            .ToArray();
    }

    private static CourseOfferingScheduleRowModel[] MapScheduleRows(
        AcademicScheduleSlotDto[] scheduleSlots,
        AcademicPeriodDefinitionDto[] termPeriods)
    {
        var periodMap = termPeriods.ToDictionary(x => x.PeriodNo);

        return scheduleSlots
            .OrderBy(x => GetWeekdaySort(x.Weekday))
            .ThenBy(x => x.PeriodNo)
            .Select(slot =>
            {
                if (!periodMap.TryGetValue(slot.PeriodNo, out var period))
                {
                    throw new UserFriendlyException("Schedule period was not found in the term.");
                }

                return new CourseOfferingScheduleRowModel(
                    FormatWeekday(slot.Weekday),
                    slot.PeriodNo,
                    period.Label,
                    period.StartTime.ToString("HH:mm"),
                    period.EndTime.ToString("HH:mm")
                );
            })
            .ToArray();
    }

    private static CourseOfferingTermPeriodOptionModel[] MapTermPeriodOptions(
        AcademicPeriodDefinitionDto[] periods)
    {
        return periods
            .Select(period => new CourseOfferingTermPeriodOptionModel(
                period.PeriodNo,
                period.Label,
                period.StartTime.ToString("HH:mm"),
                period.EndTime.ToString("HH:mm")))
            .ToArray();
    }

    private static AcademicWeekdayOptionModel[] BuildWeekdayOptions()
    {
        return
        [
            new AcademicWeekdayOptionModel(AcademicWeekday.Monday, "Monday"),
            new AcademicWeekdayOptionModel(AcademicWeekday.Tuesday, "Tuesday"),
            new AcademicWeekdayOptionModel(AcademicWeekday.Wednesday, "Wednesday"),
            new AcademicWeekdayOptionModel(AcademicWeekday.Thursday, "Thursday"),
            new AcademicWeekdayOptionModel(AcademicWeekday.Friday, "Friday"),
            new AcademicWeekdayOptionModel(AcademicWeekday.Saturday, "Saturday"),
            new AcademicWeekdayOptionModel(AcademicWeekday.Sunday, "Sunday")
        ];
    }

    private static string FormatScheduleSummary(AcademicScheduleSlotDto[] scheduleSlots)
    {
        if (scheduleSlots.Length == 0)
        {
            return "Unscheduled";
        }

        return string.Join(
            "; ",
            scheduleSlots
                .GroupBy(slot => slot.Weekday)
                .OrderBy(group => GetWeekdaySort(group.Key))
                .Select(group =>
                    $"{FormatWeekdayShort(group.Key)} {string.Join(", ", group.OrderBy(x => x.PeriodNo).Select(x => x.PeriodNo))}")
        );
    }

    private static string FormatTermTitle(int academicYear, string code, string name)
    {
        return $"{academicYear} {name} ({code})";
    }

    private static string FormatWeekday(AcademicWeekday weekday)
    {
        return weekday switch
        {
            AcademicWeekday.Monday => "Monday",
            AcademicWeekday.Tuesday => "Tuesday",
            AcademicWeekday.Wednesday => "Wednesday",
            AcademicWeekday.Thursday => "Thursday",
            AcademicWeekday.Friday => "Friday",
            AcademicWeekday.Saturday => "Saturday",
            AcademicWeekday.Sunday => "Sunday",
            _ => throw new UserFriendlyException("Weekday is invalid.")
        };
    }

    private static string FormatWeekdayShort(AcademicWeekday weekday)
    {
        return weekday switch
        {
            AcademicWeekday.Monday => "Mon",
            AcademicWeekday.Tuesday => "Tue",
            AcademicWeekday.Wednesday => "Wed",
            AcademicWeekday.Thursday => "Thu",
            AcademicWeekday.Friday => "Fri",
            AcademicWeekday.Saturday => "Sat",
            AcademicWeekday.Sunday => "Sun",
            _ => throw new UserFriendlyException("Weekday is invalid.")
        };
    }

    private static int GetWeekdaySort(AcademicWeekday weekday)
    {
        return weekday == AcademicWeekday.Sunday ? 7 : (int)weekday;
    }

    private static void NormalizeEditor(CourseOfferingEditorModel editor)
    {
        editor.OfferingCode = editor.OfferingCode.Trim();
    }

    private static void ValidateEditor(CourseOfferingEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);

        if (!editor.AcademicTermId.HasValue || editor.AcademicTermId.Value == Guid.Empty)
        {
            editor.AddFieldError(nameof(editor.AcademicTermId), "Term is required.");
        }

        if (editor.IsCreate && (!editor.CourseDefinitionId.HasValue || editor.CourseDefinitionId.Value == Guid.Empty))
        {
            editor.AddFieldError(nameof(editor.CourseDefinitionId), "Course is required.");
        }

        var validSlots = new List<(int Index, AcademicWeekday Weekday, int PeriodNo)>();
        for (var i = 0; i < editor.ScheduleSlots.Count; i++)
        {
            var slot = editor.ScheduleSlots[i];
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(slot, new ValidationContext(slot), validationResults, true);

            foreach (var validationResult in validationResults)
            {
                var members = validationResult.MemberNames.Any()
                    ? validationResult.MemberNames.Select(member => $"ScheduleSlots[{i}].{member}")
                    : [$"ScheduleSlots[{i}]"];

                foreach (var member in members)
                {
                    editor.AddFieldError(member, validationResult.ErrorMessage ?? "Schedule slot is invalid.");
                }
            }

            if (!slot.Weekday.HasValue)
            {
                editor.AddFieldError($"ScheduleSlots[{i}].{nameof(CourseOfferingScheduleSlotEditorItem.Weekday)}",
                    "Weekday is required.");
            }

            if (!slot.PeriodNo.HasValue)
            {
                editor.AddFieldError($"ScheduleSlots[{i}].{nameof(CourseOfferingScheduleSlotEditorItem.PeriodNo)}",
                    "Period is required.");
            }

            if (slot.Weekday.HasValue && slot.PeriodNo.HasValue)
            {
                validSlots.Add((i, slot.Weekday.Value, slot.PeriodNo.Value));
            }
        }

        var duplicates = validSlots
            .GroupBy(x => (x.Weekday, x.PeriodNo))
            .Where(x => x.Count() > 1);

        foreach (var group in duplicates)
        {
            foreach (var item in group)
            {
                editor.AddFieldError(
                    $"ScheduleSlots[{item.Index}].{nameof(CourseOfferingScheduleSlotEditorItem.PeriodNo)}",
                    "Schedule slots must be unique."
                );
            }
        }
    }

    private void SetCourseOfferingReplaceUrl(Guid? termId = null, Guid? offeringId = null)
    {
        if (!termId.HasValue)
        {
            Response.Headers["HX-Replace-Url"] = Url.Page(null)!;
            return;
        }

        var routeValues = new RouteValueDictionary
        {
            [nameof(TermId)] = termId.Value
        };

        if (offeringId.HasValue)
        {
            routeValues[nameof(OfferingId)] = offeringId.Value;
        }

        Response.Headers["HX-Replace-Url"] = Url.Page(null, null, routeValues)!;
    }
}