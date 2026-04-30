using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Web.Core;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic;

[Authorize]
public class CourseDefinitionManagerModel : VerbClassPageModel
{
    public CourseDefinitionManagerModel(
        ILifetimeScope lifetimeScope,
        ICourseDefinitionAppService courseDefinitionAppService) : base(lifetimeScope)
    {
        CourseDefinitionAppService = courseDefinitionAppService;
    }

    private ICourseDefinitionAppService CourseDefinitionAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? CourseDefinitionId { get; set; }

    public CourseDefinitionManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Course Definitions";

    private static string RouteKey => nameof(CourseDefinitionId);

    public async Task OnGetAsync()
    {
        State = await BuildContentAsync(CourseDefinitionId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid courseDefinitionId)
    {
        return Partial("~/Pages/Academic/CourseDefinitionManagerPartials/_CourseDefinitionInfo.cshtml", await BuildInfoModelAsync(courseDefinitionId));
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? courseDefinitionId = null)
    {
        SetReplaceUrl(RouteKey, courseDefinitionId);
        return await BuildContentPartialAsync(courseDefinitionId);
    }

    public async Task<PartialViewResult> OnGetCreateAsync(Guid? returnCourseDefinitionId = null)
    {
        SetReplaceUrl(RouteKey);
        return await BuildContentPartialAsync(
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
            var editor = await BuildEditEditorModelAsync(courseDefinitionId);
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildContentPartialAsync(courseDefinitionId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostCreateAsync(CourseDefinitionEditorInput input)
    {
        var editor = BuildEditorModel(input, isCreate: true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(editor.ReturnCourseDefinitionId, editor);
        }

        try
        {
            var courseDefinition = await CourseDefinitionAppService.CreateAsync(ToCreateInput(editor));

            ToastInfo($"Course {courseDefinition.Name} was created.", "Course created");
            SetReplaceUrl(RouteKey, courseDefinition.Id);
            return await BuildContentPartialAsync(courseDefinition.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Course definition operation failed.");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync(editor.ReturnCourseDefinitionId, editor);
        }
    }

    public async Task<PartialViewResult> OnPostUpdateAsync(
        Guid courseDefinitionId,
        CourseDefinitionEditorInput input)
    {
        var editor = BuildEditorModel(input, isCreate: false, courseDefinitionId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildContentPartialAsync(courseDefinitionId, editor);
        }

        try
        {
            var courseDefinition = await CourseDefinitionAppService.UpdateAsync(
                courseDefinitionId,
                ToUpdateInput(editor)
            );

            ToastInfo($"Course {courseDefinition.Name} was updated.", "Course saved");
            SetReplaceUrl(RouteKey, courseDefinition.Id);
            return await BuildContentPartialAsync(courseDefinition.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Course definition operation failed.");
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildContentPartialAsync(courseDefinitionId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostActivateAsync(Guid courseDefinitionId)
    {
        try
        {
            var courseDefinition = await CourseDefinitionAppService.ActivateAsync(courseDefinitionId);
            ToastInfo($"Course {courseDefinition.Name} was activated.", "Course activated");
            SetReplaceUrl(RouteKey, courseDefinition.Id);
            return await BuildContentPartialAsync(courseDefinition.Id);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Activate failed");
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildContentPartialAsync(courseDefinitionId);
        }
    }

    public async Task<PartialViewResult> OnPostDeactivateAsync(Guid courseDefinitionId)
    {
        try
        {
            var courseDefinition = await CourseDefinitionAppService.DeactivateAsync(courseDefinitionId);
            ToastWarn($"Course {courseDefinition.Name} was deactivated.", "Course deactivated");
            SetReplaceUrl(RouteKey, courseDefinition.Id);
            return await BuildContentPartialAsync(courseDefinition.Id);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Deactivate failed");
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildContentPartialAsync(courseDefinitionId);
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid courseDefinitionId)
    {
        try
        {
            var courseDefinition = await CourseDefinitionAppService.DeleteAsync(courseDefinitionId);
            ToastWarn($"Course {courseDefinition.Name} was deleted.", "Course deleted");
            SetReplaceUrl(RouteKey);
            return await BuildContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, courseDefinitionId);
            return await BuildContentPartialAsync(courseDefinitionId);
        }
    }

    private async Task<PartialViewResult> BuildContentPartialAsync(
        Guid? activeCourseDefinitionId = null,
        CourseDefinitionEditorModel? editor = null)
    {
        return Partial("~/Pages/Academic/CourseDefinitionManagerPartials/_CourseDefinitionManagerContent.cshtml", await BuildContentAsync(activeCourseDefinitionId, editor));
    }

    private async Task<CourseDefinitionManagerContentModel> BuildContentAsync(
        Guid? activeCourseDefinitionId = null,
        CourseDefinitionEditorModel? editor = null)
    {
        var courseDefinitions = (await CourseDefinitionAppService.GetListAsync())
            .Select(courseDefinition => new CourseDefinitionListItem(
                courseDefinition.Id,
                courseDefinition.Code,
                courseDefinition.Name,
                courseDefinition.IsActive))
            .ToArray();

        if (activeCourseDefinitionId.HasValue &&
            courseDefinitions.All(courseDefinition => courseDefinition.Id != activeCourseDefinitionId.Value))
        {
            activeCourseDefinitionId = null;
        }

        CourseDefinitionInfoModel? selectedCourseDefinition = null;
        if (editor is null && activeCourseDefinitionId.HasValue)
        {
            selectedCourseDefinition = await BuildInfoModelAsync(activeCourseDefinitionId.Value);
        }

        return new CourseDefinitionManagerContentModel
        {
            CourseDefinitions = courseDefinitions,
            ActiveCourseDefinitionId = activeCourseDefinitionId,
            SelectedCourseDefinition = selectedCourseDefinition,
            Editor = editor
        };
    }

    private async Task<CourseDefinitionInfoModel> BuildInfoModelAsync(Guid courseDefinitionId)
    {
        var courseDefinition = await CourseDefinitionAppService.GetAsync(courseDefinitionId);

        return new CourseDefinitionInfoModel
        {
            Id = courseDefinition.Id,
            Code = courseDefinition.Code,
            Name = courseDefinition.Name,
            Description = courseDefinition.Description,
            IsActive = courseDefinition.IsActive
        };
    }

    private async Task<CourseDefinitionEditorModel> BuildEditEditorModelAsync(Guid courseDefinitionId)
    {
        var courseDefinition = await CourseDefinitionAppService.GetAsync(courseDefinitionId);

        return new CourseDefinitionEditorModel
        {
            CourseDefinitionId = courseDefinition.Id,
            ReturnCourseDefinitionId = courseDefinition.Id,
            Code = courseDefinition.Code,
            Name = courseDefinition.Name,
            Description = courseDefinition.Description,
            IsActive = courseDefinition.IsActive
        };
    }

    private static CourseDefinitionEditorModel BuildEditorModel(
        CourseDefinitionEditorInput input,
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
            Description = input.Description
        };
    }

    private static CreateCourseDefinitionInput ToCreateInput(CourseDefinitionEditorModel editor)
    {
        return new CreateCourseDefinitionInput
        {
            Code = editor.Code,
            Name = editor.Name,
            Description = editor.Description
        };
    }

    private static UpdateCourseDefinitionInput ToUpdateInput(CourseDefinitionEditorModel editor)
    {
        return new UpdateCourseDefinitionInput
        {
            Code = editor.Code,
            Name = editor.Name,
            Description = editor.Description
        };
    }

    private static void NormalizeEditor(CourseDefinitionEditorModel editor)
    {
        editor.Code = editor.Code.Trim();
        editor.Name = editor.Name.Trim();
        editor.Description = editor.Description?.Trim();
    }

    private static void ValidateEditor(CourseDefinitionEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);
    }
}
