using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Pages.Academic;

[Authorize]
public class CourseOfferingsModel : VerbClassPageModel
{
    private const string RouteKey = nameof(CourseOfferingId);

    private readonly ICourseDefinitionAppService _courseDefinitionAppService;
    private readonly ICourseOfferingAppService _courseOfferingAppService;

    public CourseOfferingsModel(
        ILifetimeScope lifetimeScope,
        ICourseDefinitionAppService courseDefinitionAppService,
        ICourseOfferingAppService courseOfferingAppService) : base(lifetimeScope)
    {
        _courseDefinitionAppService = courseDefinitionAppService;
        _courseOfferingAppService = courseOfferingAppService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? CourseOfferingId { get; set; }

    public CourseOfferingsContentModel State { get; private set; } = new();

    protected override string PageTitle => "Course Offerings";

    public async Task<IActionResult> OnGetAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        State = await BuildCourseOfferingsContentAsync(CourseOfferingId);
        return Page();
    }

    public async Task<IActionResult> OnGetSelectAsync(Guid courseOfferingId)
    {
        return await BuildCourseOfferingDetailResultAsync(courseOfferingId);
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? courseOfferingId = null)
    {
        SetReplaceUrl(RouteKey, courseOfferingId);
        return await BuildCourseOfferingsContentPartialAsync(courseOfferingId);
    }

    public async Task<IActionResult> OnGetCreateModalAsync(
        Guid? courseDefinitionId = null,
        string? returnTo = null)
    {
        if (!IsValidReturnTarget(returnTo))
        {
            return BadRequest("Return target is required.");
        }

        return Partial(
            "_CourseOfferingCreateModal",
            await BuildCourseOfferingModalAsync(
                courseDefinitionId,
                NormalizeReturnTarget(returnTo),
                applyDefaults: true
            )
        );
    }

    public async Task<IActionResult> OnGetEditModalAsync(
        Guid courseOfferingId,
        string? returnTo = null)
    {
        if (!IsValidReturnTarget(returnTo))
        {
            return BadRequest("Return target is required.");
        }

        try
        {
            return Partial(
                "_CourseOfferingCreateModal",
                await BuildEditCourseOfferingModalAsync(
                    courseOfferingId,
                    NormalizeReturnTarget(returnTo)
                )
            );
        }
        catch (UserFriendlyException ex)
        {
            return Content(
                $"<div class=\"modal-state\"><strong>Edit Course Offering</strong><p>{ex.Message}</p></div>",
                "text/html"
            );
        }
    }

    public async Task<IActionResult> OnPostCreateModalAsync(CourseOfferingModalModel input)
    {
        NormalizeModal(input);
        EditorValidationSupport.ApplyDataAnnotations(input);

        if (!IsValidReturnTarget(input.ReturnTo))
        {
            input.AddSummaryError("Return target is invalid.");
        }

        if (input.HasErrors)
        {
            return Partial(
                "_CourseOfferingCreateModal",
                await PopulateCourseOfferingModalAsync(input)
            );
        }

        try
        {
            var created = await _courseOfferingAppService.CreateAsync(new CreateCourseOfferingInput
            {
                CourseDefinitionId = input.CourseDefinitionId.GetValueOrDefault(),
                AcademicYear = input.AcademicYear,
                TermName = input.TermName,
                ScheduleDayOfWeek = input.ScheduleDayOfWeek,
                ScheduleStartTime = input.ScheduleStartTime,
                ScheduleEndTime = input.ScheduleEndTime,
                ScheduleLocation = input.ScheduleLocation,
                EnrollmentStartsAt = input.EnrollmentStartsAt,
                EnrollmentEndsAt = input.EnrollmentEndsAt
            });

            ToastInfo(
                $"Course offering {created.AcademicYear}/{created.TermName} was created.",
                "Course offering created"
            );
            Response.Headers["HX-Redirect"] = BuildReturnUrl(
                NormalizeReturnTarget(input.ReturnTo),
                created.CourseDefinitionId,
                created.Id
            );
            return NoContent();
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(
                input,
                ex,
                "Course offering could not be created."
            );
            return Partial(
                "_CourseOfferingCreateModal",
                await PopulateCourseOfferingModalAsync(input)
            );
        }
        catch (UserFriendlyException ex)
        {
            input.AddSummaryError(ex.Message);
            return Partial(
                "_CourseOfferingCreateModal",
                await PopulateCourseOfferingModalAsync(input)
            );
        }
    }

    public async Task<IActionResult> OnPostUpdateModalAsync(Guid courseOfferingId, CourseOfferingModalModel input)
    {
        var editor = BuildModalEditor(input, false, courseOfferingId);
        NormalizeModal(editor);
        EditorValidationSupport.ApplyDataAnnotations(editor);

        if (!IsValidReturnTarget(editor.ReturnTo))
        {
            editor.AddSummaryError("Return target is invalid.");
        }

        if (editor.HasErrors)
        {
            return Partial(
                "_CourseOfferingCreateModal",
                await PopulateCourseOfferingModalAsync(editor)
            );
        }

        try
        {
            var updated = await _courseOfferingAppService.UpdateAsync(courseOfferingId, new UpdateCourseOfferingInput
            {
                CourseDefinitionId = editor.CourseDefinitionId.GetValueOrDefault(),
                AcademicYear = editor.AcademicYear,
                TermName = editor.TermName,
                ScheduleDayOfWeek = editor.ScheduleDayOfWeek,
                ScheduleStartTime = editor.ScheduleStartTime,
                ScheduleEndTime = editor.ScheduleEndTime,
                ScheduleLocation = editor.ScheduleLocation,
                EnrollmentStartsAt = editor.EnrollmentStartsAt,
                EnrollmentEndsAt = editor.EnrollmentEndsAt
            });

            ToastInfo(
                $"Course offering {updated.AcademicYear}/{updated.TermName} was updated.",
                "Course offering saved"
            );
            Response.Headers["HX-Redirect"] = BuildReturnUrl(
                NormalizeReturnTarget(editor.ReturnTo),
                updated.CourseDefinitionId,
                updated.Id
            );
            return NoContent();
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(
                editor,
                ex,
                "Course offering could not be updated."
            );
            return Partial(
                "_CourseOfferingCreateModal",
                await PopulateCourseOfferingModalAsync(editor)
            );
        }
        catch (UserFriendlyException ex)
        {
            editor.AddSummaryError(ex.Message);
            return Partial(
                "_CourseOfferingCreateModal",
                await PopulateCourseOfferingModalAsync(editor)
            );
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid courseOfferingId)
    {
        try
        {
            var deleted = await _courseOfferingAppService.DeleteAsync(courseOfferingId);
            ToastWarn(
                $"Course offering {deleted.AcademicYear}/{deleted.TermName} was deleted.",
                "Course offering deleted"
            );
            SetReplaceUrl(RouteKey);
            return await BuildCourseOfferingsContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, courseOfferingId);
            return await BuildCourseOfferingsContentPartialAsync(courseOfferingId);
        }
    }

    private async Task<IActionResult> BuildCourseOfferingDetailResultAsync(Guid courseOfferingId)
    {
        var definitions = await _courseDefinitionAppService.GetListAsync();
        var offerings = await _courseOfferingAppService.GetListAsync();
        var detail = CoursePageMapping.BuildOfferingDetail(courseOfferingId, definitions, offerings);

        if (detail is null)
        {
            return Content(
                "<div class=\"master-detail-empty\"><p>Course offering was not found.</p></div>",
                "text/html"
            );
        }

        return Partial("_CourseOfferingDetail", detail);
    }

    private async Task<PartialViewResult> BuildCourseOfferingsContentPartialAsync(
        Guid? activeCourseOfferingId = null)
    {
        return Partial(
            "_CourseOfferingsContent",
            await BuildCourseOfferingsContentAsync(activeCourseOfferingId)
        );
    }

    private async Task<CourseOfferingsContentModel> BuildCourseOfferingsContentAsync(
        Guid? activeCourseOfferingId = null)
    {
        var definitions = await _courseDefinitionAppService.GetListAsync();
        var offerings = await _courseOfferingAppService.GetListAsync();

        if (activeCourseOfferingId.HasValue && offerings.All(x => x.Id != activeCourseOfferingId.Value))
        {
            activeCourseOfferingId = null;
        }

        return new CourseOfferingsContentModel
        {
            Offerings = CoursePageMapping.BuildOfferingListItems(offerings),
            ActiveCourseOfferingId = activeCourseOfferingId,
            SelectedOffering = CoursePageMapping.BuildOfferingDetail(
                activeCourseOfferingId,
                definitions,
                offerings
            )
        };
    }

    private async Task<CourseOfferingModalModel> BuildCourseOfferingModalAsync(
        Guid? courseDefinitionId,
        string returnTo,
        bool applyDefaults)
    {
        var model = new CourseOfferingModalModel
        {
            IsCreate = true,
            ReturnTo = returnTo
        };

        if (applyDefaults)
        {
            ApplyModalDefaults(model);
        }

        if (courseDefinitionId.HasValue)
        {
            model.CourseDefinitionId = courseDefinitionId.Value;
        }

        return await PopulateCourseOfferingModalAsync(model);
    }

    private async Task<CourseOfferingModalModel> BuildEditCourseOfferingModalAsync(
        Guid courseOfferingId,
        string returnTo)
    {
        var offering = (await _courseOfferingAppService.GetListAsync())
            .FirstOrDefault(x => x.Id == courseOfferingId)
            ?? throw new UserFriendlyException("Course offering was not found.");

        return await PopulateCourseOfferingModalAsync(new CourseOfferingModalModel
        {
            IsCreate = false,
            CourseOfferingId = offering.Id,
            ReturnTo = returnTo,
            CourseDefinitionId = offering.CourseDefinitionId,
            AcademicYear = offering.AcademicYear,
            TermName = offering.TermName,
            ScheduleDayOfWeek = offering.ScheduleDayOfWeek,
            ScheduleStartTime = offering.ScheduleStartTime,
            ScheduleEndTime = offering.ScheduleEndTime,
            ScheduleLocation = offering.ScheduleLocation,
            EnrollmentStartsAt = offering.EnrollmentStartsAt,
            EnrollmentEndsAt = offering.EnrollmentEndsAt
        });
    }

    private async Task<CourseOfferingModalModel> PopulateCourseOfferingModalAsync(
        CourseOfferingModalModel model)
    {
        var definitions = await _courseDefinitionAppService.GetListAsync();
        var definitionOptions = CoursePageMapping.BuildDefinitionOptions(definitions);

        if (model.CourseDefinitionId.HasValue && definitions.All(x => x.Id != model.CourseDefinitionId.Value))
        {
            model.CourseDefinitionId = null;
        }

        model.CourseDefinitionOptions = definitionOptions;
        return model;
    }

    private static void ApplyModalDefaults(CourseOfferingModalModel model)
    {
        model.AcademicYear = DateTime.Today.Year;
        model.EnrollmentStartsAt = DateTime.Today;
        model.EnrollmentEndsAt = DateTime.Today.AddDays(7);
    }

    private static void NormalizeModal(CourseOfferingModalModel model)
    {
        model.TermName = model.TermName.Trim();
        model.ScheduleLocation = string.IsNullOrWhiteSpace(model.ScheduleLocation)
            ? null
            : model.ScheduleLocation.Trim();
    }

    private static CourseOfferingModalModel BuildModalEditor(
        CourseOfferingModalModel input,
        bool isCreate,
        Guid? courseOfferingId = null)
    {
        return new CourseOfferingModalModel
        {
            IsCreate = isCreate,
            CourseOfferingId = courseOfferingId,
            ReturnTo = input.ReturnTo,
            CourseDefinitionId = input.CourseDefinitionId,
            AcademicYear = input.AcademicYear,
            TermName = input.TermName,
            ScheduleDayOfWeek = input.ScheduleDayOfWeek,
            ScheduleStartTime = input.ScheduleStartTime,
            ScheduleEndTime = input.ScheduleEndTime,
            ScheduleLocation = input.ScheduleLocation,
            EnrollmentStartsAt = input.EnrollmentStartsAt,
            EnrollmentEndsAt = input.EnrollmentEndsAt
        };
    }

    private string BuildReturnUrl(string returnTo, Guid courseDefinitionId, Guid courseOfferingId)
    {
        return returnTo switch
        {
            CourseOfferingReturnTargets.Definitions => Url.Page(
                "/Academic/CourseDefinitions",
                null,
                new { courseDefinitionId }
            ) ?? throw new AbpException("Course definitions return URL could not be created."),
            CourseOfferingReturnTargets.Offerings => Url.Page(
                "/Academic/CourseOfferings",
                null,
                new { courseOfferingId }
            ) ?? throw new AbpException("Course offerings return URL could not be created."),
            _ => throw new AbpException("Course offering return target is invalid.")
        };
    }

    private static bool IsValidReturnTarget(string? returnTo)
    {
        return string.Equals(returnTo, CourseOfferingReturnTargets.Definitions, StringComparison.OrdinalIgnoreCase)
            || string.Equals(returnTo, CourseOfferingReturnTargets.Offerings, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeReturnTarget(string? returnTo)
    {
        return string.Equals(returnTo, CourseOfferingReturnTargets.Definitions, StringComparison.OrdinalIgnoreCase)
            ? CourseOfferingReturnTargets.Definitions
            : CourseOfferingReturnTargets.Offerings;
    }
}
