using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic;

[Authorize]
public class CourseParticipantManagerModel : VerbClassPageModel
{
    public CourseParticipantManagerModel(
        ILifetimeScope lifetimeScope,
        IAcademicTermAppService academicTermAppService,
        ICourseOfferingAppService courseOfferingAppService,
        ICourseOfferingParticipantAppService participantAppService) : base(lifetimeScope)
    {
        AcademicTermAppService = academicTermAppService;
        CourseOfferingAppService = courseOfferingAppService;
        ParticipantAppService = participantAppService;
    }

    private IAcademicTermAppService AcademicTermAppService { get; }

    private ICourseOfferingAppService CourseOfferingAppService { get; }

    private ICourseOfferingParticipantAppService ParticipantAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? TermId { get; set; }

    [BindProperty(SupportsGet = true)] public Guid? OfferingId { get; set; }

    public CourseParticipantManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Course Participants";

    public async Task OnGetAsync()
    {
        State = await BuildContentAsync(TermId, OfferingId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid offeringId)
    {
        return Partial("~/Pages/Academic/CourseParticipantManagerPartials/_CourseParticipantDetail.cshtml",
            await BuildDetailModelAsync(offeringId));
    }

    public async Task<PartialViewResult> OnGetTermAsync(Guid? termId = null)
    {
        SetCourseParticipantReplaceUrl(termId);
        return await BuildContentPartialAsync(termId);
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? termId = null, Guid? offeringId = null)
    {
        SetCourseParticipantReplaceUrl(termId, offeringId);
        return await BuildContentPartialAsync(termId, offeringId);
    }

    public async Task<PartialViewResult> OnPostAddAsync(Guid courseOfferingId, CourseParticipantEditorInput input)
    {
        var editor = await BuildEditorModelAsync(courseOfferingId, input);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetCourseParticipantReplaceUrl(editor.ReturnTermId, editor.ReturnOfferingId);
            return await BuildContentPartialAsync(editor.ReturnTermId, editor.ReturnOfferingId, editor);
        }

        try
        {
            var participant = await ParticipantAppService.AddAsync(new AddCourseOfferingParticipantInput
            {
                CourseOfferingId = courseOfferingId,
                UserId = editor.UserId,
                Role = editor.Role!.Value
            });

            ToastInfo($"{participant.DisplayName} was assigned.", "Participant added");
            SetCourseParticipantReplaceUrl(editor.ReturnTermId, participant.CourseOfferingId);
            return await BuildContentPartialAsync(editor.ReturnTermId, participant.CourseOfferingId);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Course participant operation failed.");
            SetCourseParticipantReplaceUrl(editor.ReturnTermId, editor.ReturnOfferingId);
            return await BuildContentPartialAsync(editor.ReturnTermId, editor.ReturnOfferingId, editor);
        }
    }

    public async Task<PartialViewResult> OnPostChangeRoleAsync(
        Guid participantId,
        Guid termId,
        Guid offeringId,
        CourseOfferingParticipantRole role)
    {
        try
        {
            var participant = await ParticipantAppService.ChangeRoleAsync(
                participantId,
                new ChangeCourseOfferingParticipantRoleInput
                {
                    Role = role
                }
            );

            ToastInfo($"{participant.DisplayName} was updated.", "Participant saved");
            SetCourseParticipantReplaceUrl(termId, offeringId);
            return await BuildContentPartialAsync(termId, offeringId);
        }
        catch (AbpValidationException ex)
        {
            ToastError(FirstValidationMessage(ex), "Update failed");
            SetCourseParticipantReplaceUrl(termId, offeringId);
            return await BuildContentPartialAsync(termId, offeringId);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetCourseParticipantReplaceUrl(termId, offeringId);
            return await BuildContentPartialAsync(termId, offeringId);
        }
    }

    public async Task<PartialViewResult> OnPostRemoveAsync(Guid participantId, Guid termId, Guid offeringId)
    {
        try
        {
            var participant = await ParticipantAppService.RemoveAsync(participantId);
            ToastWarn($"{participant.DisplayName} was removed.", "Participant removed");
            SetCourseParticipantReplaceUrl(termId, offeringId);
            return await BuildContentPartialAsync(termId, offeringId);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Remove failed");
            SetCourseParticipantReplaceUrl(termId, offeringId);
            return await BuildContentPartialAsync(termId, offeringId);
        }
    }

    private async Task<PartialViewResult> BuildContentPartialAsync(
        Guid? activeTermId = null,
        Guid? activeOfferingId = null,
        CourseParticipantEditorModel? editor = null)
    {
        return Partial(
            "~/Pages/Academic/CourseParticipantManagerPartials/_CourseParticipantManagerContent.cshtml",
            await BuildContentAsync(activeTermId, activeOfferingId, editor)
        );
    }

    private async Task<CourseParticipantManagerContentModel> BuildContentAsync(
        Guid? activeTermId = null,
        Guid? activeOfferingId = null,
        CourseParticipantEditorModel? editor = null)
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

        CourseParticipantDetailModel? detail = null;
        if (activeOfferingId.HasValue)
        {
            detail = await BuildDetailModelAsync(activeOfferingId.Value, editor);
        }

        return new CourseParticipantManagerContentModel
        {
            Terms = terms,
            ActiveTermId = activeTermId,
            Offerings = offerings,
            ActiveOfferingId = activeOfferingId,
            Detail = detail
        };
    }

    private async Task<CourseParticipantDetailModel> BuildDetailModelAsync(
        Guid courseOfferingId,
        CourseParticipantEditorModel? editor = null)
    {
        var offering = await CourseOfferingAppService.GetAsync(courseOfferingId);
        var roleOptions = BuildRoleOptions();

        return new CourseParticipantDetailModel
        {
            CourseOfferingId = offering.Id,
            AcademicTermId = offering.AcademicTermId,
            OfferingTitle = $"{offering.OfferingCode} {offering.CourseNameSnapshot}",
            Participants = (await ParticipantAppService.GetListAsync(courseOfferingId))
                .Select(participant => new CourseParticipantListItemModel
                {
                    Id = participant.Id,
                    CourseOfferingId = participant.CourseOfferingId,
                    UserId = participant.UserId,
                    DisplayName = participant.DisplayName,
                    UserName = participant.UserName,
                    Email = participant.Email,
                    Role = participant.Role
                })
                .ToArray(),
            RoleOptions = roleOptions,
            Editor = editor ?? await BuildCreateEditorModelAsync(offering.Id, offering.AcademicTermId)
        };
    }

    private async Task<CourseParticipantEditorModel> BuildCreateEditorModelAsync(
        Guid courseOfferingId,
        Guid termId)
    {
        return new CourseParticipantEditorModel
        {
            CourseOfferingId = courseOfferingId,
            ReturnTermId = termId,
            ReturnOfferingId = courseOfferingId,
            RoleOptions = BuildRoleOptions(),
            UserOptions = await BuildUserOptionsAsync(courseOfferingId)
        };
    }

    private async Task<CourseParticipantEditorModel> BuildEditorModelAsync(
        Guid courseOfferingId,
        CourseParticipantEditorInput input)
    {
        return new CourseParticipantEditorModel
        {
            CourseOfferingId = courseOfferingId,
            UserId = input.UserId,
            Role = input.Role,
            ReturnTermId = input.ReturnTermId,
            ReturnOfferingId = input.ReturnOfferingId,
            RoleOptions = BuildRoleOptions(),
            UserOptions = await BuildUserOptionsAsync(courseOfferingId)
        };
    }

    private async Task<CourseParticipantUserOptionModel[]> BuildUserOptionsAsync(Guid courseOfferingId)
    {
        return (await ParticipantAppService.GetAssignableUsersAsync(courseOfferingId))
            .Select(user => new CourseParticipantUserOptionModel(
                user.UserId,
                user.DisplayName,
                user.UserName,
                user.Email,
                user.RoleNames))
            .ToArray();
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

    private static CourseParticipantRoleOptionModel[] BuildRoleOptions()
    {
        return
        [
            new CourseParticipantRoleOptionModel(CourseOfferingParticipantRole.Teacher, "Teacher"),
            new CourseParticipantRoleOptionModel(CourseOfferingParticipantRole.Assistant, "Assistant"),
            new CourseParticipantRoleOptionModel(CourseOfferingParticipantRole.Student, "Student"),
            new CourseParticipantRoleOptionModel(CourseOfferingParticipantRole.Observer, "Observer")
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

    private static void ValidateEditor(CourseParticipantEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);

        if (!editor.UserId.HasValue || editor.UserId.Value == Guid.Empty)
        {
            editor.AddFieldError(nameof(editor.UserId), "User is required.");
        }

        if (!editor.Role.HasValue || !Enum.IsDefined(editor.Role.Value))
        {
            editor.AddFieldError(nameof(editor.Role), "Role is required.");
        }
    }

    private static string FirstValidationMessage(AbpValidationException ex)
    {
        return ex.ValidationErrors.FirstOrDefault()?.ErrorMessage
               ?? "Course participant operation failed.";
    }

    private void SetCourseParticipantReplaceUrl(Guid? termId = null, Guid? offeringId = null)
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