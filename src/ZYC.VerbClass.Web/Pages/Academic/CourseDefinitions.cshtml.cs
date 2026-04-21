using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Pages.Academic;

[Authorize]
public class CourseDefinitionsModel : VerbClassPageModel
{
    private const string RouteKey = nameof(CourseDefinitionId);

    private readonly ICourseDefinitionAppService _courseDefinitionAppService;
    private readonly ICourseOfferingAppService _courseOfferingAppService;

    public CourseDefinitionsModel(
        ILifetimeScope lifetimeScope,
        ICourseDefinitionAppService courseDefinitionAppService,
        ICourseOfferingAppService courseOfferingAppService) : base(lifetimeScope)
    {
        _courseDefinitionAppService = courseDefinitionAppService;
        _courseOfferingAppService = courseOfferingAppService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? CourseDefinitionId { get; set; }

    public CourseDefinitionsContentModel State { get; private set; } = new();

    protected override string PageTitle => "Course Definitions";

    public async Task<IActionResult> OnGetAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        State = await BuildCourseDefinitionsContentAsync(CourseDefinitionId);
        return Page();
    }

    public async Task<IActionResult> OnGetSelectAsync(Guid courseDefinitionId)
    {
        return await BuildCourseDefinitionDetailResultAsync(courseDefinitionId);
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? courseDefinitionId = null)
    {
        SetReplaceUrl(RouteKey, courseDefinitionId);
        return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId);
    }

    public async Task<PartialViewResult> OnGetCreateAsync(Guid? returnCourseDefinitionId = null)
    {
        SetReplaceUrl(RouteKey);
        return await BuildCourseDefinitionsContentPartialAsync(
            returnCourseDefinitionId,
            new CourseDefinitionEditorModel
            {
                IsCreate = true,
                IsActive = true,
                ReturnCourseDefinitionId = returnCourseDefinitionId
            }
        );
    }

    public async Task<IActionResult> OnGetEditAsync(Guid courseDefinitionId)
    {
        try
        {
            var editor = await BuildEditEditorAsync(courseDefinitionId);
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey);
            return await BuildCourseDefinitionsContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostCreateAsync(CourseDefinitionEditorModel input)
    {
        var editor = BuildEditor(input, isCreate: true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey);
            return await BuildCourseDefinitionsContentPartialAsync(
                editor.ReturnCourseDefinitionId,
                editor
            );
        }

        try
        {
            var created = await _courseDefinitionAppService.CreateAsync(new CreateCourseDefinitionInput
            {
                Code = editor.Code,
                Name = editor.Name,
                ShortName = editor.ShortName,
                Description = editor.Description,
                IsActive = editor.IsActive
            });

            ToastInfo($"Course definition {created.Code} was created.", "Course definition created");
            SetReplaceUrl(RouteKey, created.Id);
            return await BuildCourseDefinitionsContentPartialAsync(created.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(
                editor,
                ex,
                "Course definition could not be created."
            );
            SetReplaceUrl(RouteKey);
            return await BuildCourseDefinitionsContentPartialAsync(
                editor.ReturnCourseDefinitionId,
                editor
            );
        }
        catch (UserFriendlyException ex)
        {
            editor.AddSummaryError(ex.Message);
            SetReplaceUrl(RouteKey);
            return await BuildCourseDefinitionsContentPartialAsync(
                editor.ReturnCourseDefinitionId,
                editor
            );
        }
    }

    public async Task<PartialViewResult> OnPostUpdateAsync(Guid courseDefinitionId, CourseDefinitionEditorModel input)
    {
        var editor = BuildEditor(input, isCreate: false, courseDefinitionId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId, editor);
        }

        try
        {
            var updated = await _courseDefinitionAppService.UpdateAsync(courseDefinitionId, new UpdateCourseDefinitionInput
            {
                Code = editor.Code,
                Name = editor.Name,
                ShortName = editor.ShortName,
                Description = editor.Description,
                IsActive = editor.IsActive
            });

            ToastInfo($"Course definition {updated.Code} was updated.", "Course definition saved");
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(
                editor,
                ex,
                "Course definition could not be updated."
            );
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId, editor);
        }
        catch (UserFriendlyException ex)
        {
            editor.AddSummaryError(ex.Message);
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId, editor);
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid courseDefinitionId)
    {
        try
        {
            var deleted = await _courseDefinitionAppService.DeleteAsync(courseDefinitionId);
            ToastWarn(
                $"Course definition {deleted.Code} was deleted.",
                "Course definition deleted"
            );
            SetReplaceUrl(RouteKey);
            return await BuildCourseDefinitionsContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildCourseDefinitionsContentPartialAsync(courseDefinitionId);
        }
    }

    private async Task<IActionResult> BuildCourseDefinitionDetailResultAsync(Guid courseDefinitionId)
    {
        var definitions = await _courseDefinitionAppService.GetListAsync();
        var offerings = await _courseOfferingAppService.GetListAsync();
        var detail = CoursePageMapping.BuildDefinitionDetail(courseDefinitionId, definitions, offerings);

        if (detail is null)
        {
            return Content(
                "<div class=\"master-detail-empty\"><p>Course definition was not found.</p></div>",
                "text/html"
            );
        }

        return Partial("_CourseDefinitionDetail", detail);
    }

    private async Task<PartialViewResult> BuildCourseDefinitionsContentPartialAsync(
        Guid? activeCourseDefinitionId = null,
        CourseDefinitionEditorModel? editor = null)
    {
        return Partial(
            "_CourseDefinitionsContent",
            await BuildCourseDefinitionsContentAsync(activeCourseDefinitionId, editor)
        );
    }

    private async Task<CourseDefinitionsContentModel> BuildCourseDefinitionsContentAsync(
        Guid? activeCourseDefinitionId = null,
        CourseDefinitionEditorModel? editor = null)
    {
        var definitions = await _courseDefinitionAppService.GetListAsync();
        var offerings = await _courseOfferingAppService.GetListAsync();

        if (activeCourseDefinitionId.HasValue && definitions.All(x => x.Id != activeCourseDefinitionId.Value))
        {
            activeCourseDefinitionId = null;
        }

        if (!activeCourseDefinitionId.HasValue && editor?.IsCreate == false)
        {
            activeCourseDefinitionId = editor.CourseDefinitionId;
        }

        return new CourseDefinitionsContentModel
        {
            Definitions = CoursePageMapping.BuildDefinitionListItems(definitions, offerings),
            ActiveCourseDefinitionId = activeCourseDefinitionId,
            SelectedDefinition = editor is null
                ? CoursePageMapping.BuildDefinitionDetail(activeCourseDefinitionId, definitions, offerings)
                : null,
            Editor = editor
        };
    }

    private async Task<CourseDefinitionEditorModel> BuildEditEditorAsync(Guid courseDefinitionId)
    {
        var definition = (await _courseDefinitionAppService.GetListAsync())
            .FirstOrDefault(x => x.Id == courseDefinitionId)
            ?? throw new UserFriendlyException("Course definition was not found.");

        return new CourseDefinitionEditorModel
        {
            IsCreate = false,
            CourseDefinitionId = definition.Id,
            ReturnCourseDefinitionId = definition.Id,
            Code = definition.Code,
            Name = definition.Name,
            ShortName = definition.ShortName,
            Description = definition.Description,
            IsActive = definition.IsActive
        };
    }

    private static CourseDefinitionEditorModel BuildEditor(
        CourseDefinitionEditorModel input,
        bool isCreate,
        Guid? courseDefinitionId = null)
    {
        return new CourseDefinitionEditorModel
        {
            IsCreate = isCreate,
            CourseDefinitionId = courseDefinitionId,
            ReturnCourseDefinitionId = input.ReturnCourseDefinitionId,
            Code = input.Code,
            Name = input.Name,
            ShortName = input.ShortName,
            Description = input.Description,
            IsActive = input.IsActive
        };
    }

    private static void NormalizeEditor(CourseDefinitionEditorModel editor)
    {
        editor.Code = editor.Code.Trim();
        editor.Name = editor.Name.Trim();
        editor.ShortName = string.IsNullOrWhiteSpace(editor.ShortName)
            ? null
            : editor.ShortName.Trim();
        editor.Description = string.IsNullOrWhiteSpace(editor.Description)
            ? null
            : editor.Description.Trim();
    }

    private static void ValidateEditor(CourseDefinitionEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);
    }
}
