using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.Departments;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Admin;

[Authorize(VerbClassPermissions.Departments.Access)]
public class DepartmentManagerModel : VerbClassPageModel
{
    public DepartmentManagerModel(
        ILifetimeScope lifetimeScope,
        IDepartmentAppService departmentAppService) : base(lifetimeScope)
    {
        DepartmentAppService = departmentAppService;
    }

    private IDepartmentAppService DepartmentAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? DepartmentId { get; set; }

    public DepartmentManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Department Manager";

    private static string RouteKey => nameof(DepartmentId);

    public async Task OnGetAsync()
    {
        State = await BuildDepartmentManagerContentAsync(DepartmentId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid departmentId)
    {
        return await BuildDepartmentInfoPartialAsync(departmentId);
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? departmentId = null)
    {
        SetReplaceUrl(RouteKey, departmentId);
        return await BuildDepartmentManagerContentPartialAsync(departmentId);
    }

    public async Task<IActionResult> OnGetCreateAsync(
        Guid? parentDepartmentId = null,
        Guid? returnDepartmentId = null)
    {
        if (!(await DepartmentAppService.GetPermissionsAsync()).CanCreate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        SetReplaceUrl(RouteKey);
        return await BuildDepartmentManagerContentPartialAsync(
            parentDepartmentId ?? returnDepartmentId,
            BuildCreateEditorModel(parentDepartmentId, returnDepartmentId)
        );
    }

    public async Task<IActionResult> OnGetEditAsync(Guid departmentId)
    {
        if (!(await DepartmentAppService.GetPermissionsAsync()).CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        try
        {
            var editor = await BuildEditEditorModelAsync(departmentId);
            SetReplaceUrl(RouteKey, departmentId);
            return await BuildDepartmentManagerContentPartialAsync(departmentId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey);
            return await BuildDepartmentManagerContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostCreateAsync(DepartmentEditorInput input)
    {
        var editor = BuildEditorModel(input, true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey);
            return await BuildDepartmentManagerContentPartialAsync(
                editor.ParentDepartmentId ?? editor.ReturnDepartmentId,
                editor
            );
        }

        try
        {
            var department = await DepartmentAppService.CreateAsync(ToCreateInput(editor));

            ToastInfo($"Department {department.Name} was created.", "Department created");
            SetReplaceUrl(RouteKey, department.Id);
            return await BuildDepartmentManagerContentPartialAsync(department.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Department operation failed.");
            SetReplaceUrl(RouteKey);
            return await BuildDepartmentManagerContentPartialAsync(
                editor.ParentDepartmentId ?? editor.ReturnDepartmentId,
                editor
            );
        }
    }

    public async Task<PartialViewResult> OnPostUpdateAsync(Guid departmentId, DepartmentEditorInput input)
    {
        var editor = BuildEditorModel(input, false, departmentId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, departmentId);
            return await BuildDepartmentManagerContentPartialAsync(departmentId, editor);
        }

        try
        {
            var department = await DepartmentAppService.UpdateAsync(departmentId, ToUpdateInput(editor));
            ToastInfo($"Department {department.Name} was updated.", "Department saved");
            SetReplaceUrl(RouteKey, departmentId);
            return await BuildDepartmentManagerContentPartialAsync(departmentId);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Department operation failed.");
            SetReplaceUrl(RouteKey, departmentId);
            return await BuildDepartmentManagerContentPartialAsync(departmentId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetReplaceUrl(RouteKey);
            return await BuildDepartmentManagerContentPartialAsync();
        }
    }

    public async Task<PartialViewResult> OnPostDeleteAsync(Guid departmentId)
    {
        try
        {
            var department = await DepartmentAppService.DeleteAsync(departmentId);
            ToastWarn($"Department {department.Name} was deleted.", "Department deleted");
            SetReplaceUrl(RouteKey);
            return await BuildDepartmentManagerContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, departmentId);
            return await BuildDepartmentManagerContentPartialAsync(departmentId);
        }
    }

    private async Task<PartialViewResult> BuildDepartmentInfoPartialAsync(Guid departmentId)
    {
        var model = await BuildDepartmentInfoModelAsync(departmentId);
        return Partial("_DepartmentInfo", model);
    }

    private async Task<DepartmentInfoModel> BuildDepartmentInfoModelAsync(Guid departmentId)
    {
        var department = await DepartmentAppService.GetAsync(departmentId);

        return new DepartmentInfoModel
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            ShortName = department.ShortName,
            ParentDepartmentName = department.ParentDepartmentName,
            PathDisplay = department.PathDisplay,
            Sort = department.Sort,
            IsActive = department.IsActive,
            CanAssignUsers = department.CanAssignUsers,
            CurrentUserCount = department.CurrentUserCount,
            EffectiveFrom = department.EffectiveFrom,
            EffectiveTo = department.EffectiveTo,
            CreationTime = department.CreationTime,
            LastModificationTime = department.LastModificationTime,
            HasChildren = department.HasChildren,
            CanCreateChild = department.CanCreateChild,
            CanUpdate = department.CanUpdate,
            CanDelete = department.CanDelete
        };
    }

    private async Task<PartialViewResult> BuildDepartmentManagerContentPartialAsync(
        Guid? activeDepartmentId = null,
        DepartmentEditorModel? editor = null)
    {
        var model = await BuildDepartmentManagerContentAsync(activeDepartmentId, editor);
        return Partial("_DepartmentManagerContent", model);
    }

    private async Task<DepartmentManagerContentModel> BuildDepartmentManagerContentAsync(
        Guid? activeDepartmentId = null,
        DepartmentEditorModel? editor = null)
    {
        var permissions = await DepartmentAppService.GetPermissionsAsync();
        var departments = (await DepartmentAppService.GetListAsync())
            .Select(x => new DepartmentListItem(
                x.Id,
                x.Name,
                x.Code,
                x.ShortName,
                x.PathDisplay,
                x.Depth,
                x.IsActive,
                x.CanAssignUsers,
                x.CurrentUserCount))
            .ToArray();

        if (activeDepartmentId.HasValue && departments.All(x => x.Id != activeDepartmentId.Value))
        {
            activeDepartmentId = null;
        }

        if (editor is not null)
        {
            await PopulateParentOptionsAsync(editor);
        }

        DepartmentInfoModel? selectedDepartment = null;
        if (editor is null && activeDepartmentId.HasValue)
        {
            selectedDepartment = await BuildDepartmentInfoModelAsync(activeDepartmentId.Value);
        }

        return new DepartmentManagerContentModel
        {
            Departments = departments,
            ActiveDepartmentId = activeDepartmentId,
            SelectedDepartment = selectedDepartment,
            Editor = editor,
            CanCreate = permissions.CanCreate
        };
    }

    private static DepartmentEditorModel BuildCreateEditorModel(
        Guid? parentDepartmentId = null,
        Guid? returnDepartmentId = null)
    {
        return new DepartmentEditorModel
        {
            IsCreate = true,
            ParentDepartmentId = parentDepartmentId,
            IsActive = true,
            CanAssignUsers = true,
            ReturnDepartmentId = returnDepartmentId
        };
    }

    private async Task<DepartmentEditorModel> BuildEditEditorModelAsync(Guid departmentId)
    {
        var department = await DepartmentAppService.GetEditorAsync(departmentId);

        return new DepartmentEditorModel
        {
            IsCreate = false,
            DepartmentId = department.Id,
            Code = department.Code,
            Name = department.Name,
            ShortName = department.ShortName,
            ParentDepartmentId = department.ParentDepartmentId,
            Sort = department.Sort,
            IsActive = department.IsActive,
            CanAssignUsers = department.CanAssignUsers,
            EffectiveFrom = department.EffectiveFrom,
            EffectiveTo = department.EffectiveTo,
            ReturnDepartmentId = department.Id
        };
    }

    private static DepartmentEditorModel BuildEditorModel(
        DepartmentEditorInput input,
        bool isCreate,
        Guid? departmentId = null)
    {
        return new DepartmentEditorModel
        {
            IsCreate = isCreate,
            DepartmentId = departmentId,
            Code = input.Code,
            Name = input.Name,
            ShortName = input.ShortName,
            ParentDepartmentId = input.ParentDepartmentId,
            Sort = input.Sort,
            IsActive = input.IsActive,
            CanAssignUsers = input.CanAssignUsers,
            EffectiveFrom = input.EffectiveFrom,
            EffectiveTo = input.EffectiveTo,
            ReturnDepartmentId = input.ReturnDepartmentId
        };
    }

    private static CreateDepartmentInput ToCreateInput(DepartmentEditorModel editor)
    {
        return new CreateDepartmentInput
        {
            Code = editor.Code,
            Name = editor.Name,
            ShortName = editor.ShortName,
            ParentDepartmentId = editor.ParentDepartmentId,
            Sort = editor.Sort,
            IsActive = editor.IsActive,
            CanAssignUsers = editor.CanAssignUsers,
            EffectiveFrom = editor.EffectiveFrom,
            EffectiveTo = editor.EffectiveTo
        };
    }

    private static UpdateDepartmentInput ToUpdateInput(DepartmentEditorModel editor)
    {
        return new UpdateDepartmentInput
        {
            Code = editor.Code,
            Name = editor.Name,
            ShortName = editor.ShortName,
            ParentDepartmentId = editor.ParentDepartmentId,
            Sort = editor.Sort,
            IsActive = editor.IsActive,
            CanAssignUsers = editor.CanAssignUsers,
            EffectiveFrom = editor.EffectiveFrom,
            EffectiveTo = editor.EffectiveTo
        };
    }

    private static void NormalizeEditor(DepartmentEditorModel editor)
    {
        editor.Code = editor.Code.Trim();
        editor.Name = editor.Name.Trim();
        editor.ShortName = string.IsNullOrWhiteSpace(editor.ShortName)
            ? null
            : editor.ShortName.Trim();
    }

    private static void ValidateEditor(DepartmentEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);
    }

    private async Task PopulateParentOptionsAsync(DepartmentEditorModel editor)
    {
        editor.ParentOptions.Clear();

        var options = await DepartmentAppService.GetParentOptionsAsync(editor.DepartmentId);
        foreach (var option in options)
        {
            editor.ParentOptions.Add(new DepartmentParentOption(option.Id, option.Label));
        }
    }
}
