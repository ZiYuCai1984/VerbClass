using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.Roles;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Admin;

[Authorize(VerbClassPermissions.Roles.Access)]
public class RoleManagerModel : VerbClassPageModel
{
    public RoleManagerModel(
        ILifetimeScope lifetimeScope,
        IRoleManagementAppService roleManagementAppService) : base(lifetimeScope)
    {
        RoleManagementAppService = roleManagementAppService;
    }

    private IRoleManagementAppService RoleManagementAppService { get; }

    [BindProperty(SupportsGet = true)] public string? RoleName { get; set; }

    public RoleManagerContentModel State { get; private set; } = new();

    protected override string PageTitle => "Role Manager";

    private static string RouteKey => nameof(RoleName);

    public async Task OnGetAsync()
    {
        State = await BuildRoleManagerContentAsync(RoleName);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(string roleName)
    {
        return await BuildRoleInfoPartialAsync(roleName);
    }

    public async Task<PartialViewResult> OnGetViewAsync(string? roleName = null)
    {
        SetReplaceUrl(RouteKey, roleName);
        return await BuildRoleManagerContentPartialAsync(roleName);
    }

    public async Task<IActionResult> OnGetEditAsync(string roleName)
    {
        if (!(await RoleManagementAppService.GetPermissionsAsync()).CanManagePermissions)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        try
        {
            var editor = await BuildEditEditorModelAsync(roleName);
            SetReplaceUrl(RouteKey, editor.RoleName);
            return await BuildRoleManagerContentPartialAsync(editor.RoleName, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey, roleName);
            return await BuildRoleManagerContentPartialAsync(roleName);
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(string roleName, RolePermissionEditorInput input)
    {
        if (!(await RoleManagementAppService.GetPermissionsAsync()).CanManagePermissions)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        var editor = BuildEditorModel(input, roleName);
        NormalizeEditor(editor);
        ValidateEditor(editor);
        await PopulatePermissionsAsync(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, editor.RoleName);
            return await BuildRoleManagerContentPartialAsync(editor.RoleName, editor);
        }

        try
        {
            var role = await RoleManagementAppService.UpdatePermissionsAsync(editor.RoleName, ToUpdateInput(editor));

            ToastInfo($"Permissions for {role.Name} were updated.", "Role saved");
            SetReplaceUrl(RouteKey, role.Name);
            return await BuildRoleManagerContentPartialAsync(role.Name);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "Role operation failed.");
            await PopulatePermissionsAsync(editor);
            SetReplaceUrl(RouteKey, editor.RoleName);
            return await BuildRoleManagerContentPartialAsync(editor.RoleName, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetReplaceUrl(RouteKey, editor.RoleName);
            return await BuildRoleManagerContentPartialAsync(editor.RoleName);
        }
    }

    private async Task<PartialViewResult> BuildRoleInfoPartialAsync(string roleName)
    {
        return Partial("_RoleInfo", await BuildRoleInfoModelAsync(roleName));
    }

    private async Task<RoleInfoModel> BuildRoleInfoModelAsync(string roleName)
    {
        var role = await RoleManagementAppService.GetAsync(roleName);

        return new RoleInfoModel
        {
            Name = role.Name,
            UserCount = role.UserCount,
            GrantedPermissionCount = role.GrantedPermissionCount,
            AvailablePermissionCount = role.AvailablePermissionCount,
            GrantedPermissions = role.GrantedPermissions,
            AssignedUsers = role.AssignedUsers,
            RemainingUserCount = role.RemainingUserCount,
            CanManagePermissions = role.CanManagePermissions
        };
    }

    private async Task<PartialViewResult> BuildRoleManagerContentPartialAsync(
        string? activeRoleName = null,
        RolePermissionEditorModel? editor = null)
    {
        return Partial("_RoleManagerContent", await BuildRoleManagerContentAsync(activeRoleName, editor));
    }

    private async Task<RoleManagerContentModel> BuildRoleManagerContentAsync(
        string? activeRoleName = null,
        RolePermissionEditorModel? editor = null)
    {
        var roles = (await RoleManagementAppService.GetListAsync())
            .Select(role => new RoleListItem(
                role.Name,
                role.UserCount,
                role.GrantedPermissionCount,
                role.AvailablePermissionCount
            ))
            .ToArray();

        if (!string.IsNullOrWhiteSpace(activeRoleName) &&
            roles.All(role => !string.Equals(role.Name, activeRoleName, StringComparison.OrdinalIgnoreCase)))
        {
            activeRoleName = null;
        }

        RoleInfoModel? selectedRole = null;
        if (editor is null && !string.IsNullOrWhiteSpace(activeRoleName))
        {
            selectedRole = await BuildRoleInfoModelAsync(activeRoleName);
        }

        return new RoleManagerContentModel
        {
            Roles = roles,
            ActiveRoleName = activeRoleName,
            SelectedRole = selectedRole,
            Editor = editor
        };
    }

    private async Task<RolePermissionEditorModel> BuildEditEditorModelAsync(string roleName)
    {
        var role = await RoleManagementAppService.GetEditorAsync(roleName);
        var grantedPermissionNames = role.Permissions
            .Where(permission => permission.IsGranted)
            .Select(permission => permission.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new RolePermissionEditorModel
        {
            RoleName = role.Name,
            GrantedPermissionNames = grantedPermissionNames,
            Permissions = MergePermissionSelection(role.Permissions,
                grantedPermissionNames.ToHashSet(StringComparer.Ordinal))
        };
    }

    private static RolePermissionEditorModel BuildEditorModel(RolePermissionEditorInput input, string roleName)
    {
        return new RolePermissionEditorModel
        {
            RoleName = string.IsNullOrWhiteSpace(input.RoleName) ? roleName : input.RoleName,
            GrantedPermissionNames = input.GrantedPermissionNames
        };
    }

    private static UpdateRolePermissionsInput ToUpdateInput(RolePermissionEditorModel editor)
    {
        return new UpdateRolePermissionsInput
        {
            GrantedPermissionNames = editor.GrantedPermissionNames
        };
    }

    private async Task PopulatePermissionsAsync(RolePermissionEditorModel editor)
    {
        var definition = await RoleManagementAppService.GetEditorAsync(editor.RoleName);
        editor.Permissions = MergePermissionSelection(
            definition.Permissions,
            editor.GrantedPermissionNames.ToHashSet(StringComparer.Ordinal)
        );
    }

    private static void NormalizeEditor(RolePermissionEditorModel editor)
    {
        editor.RoleName = editor.RoleName.Trim();
        editor.GrantedPermissionNames = editor.GrantedPermissionNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateEditor(RolePermissionEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);
    }

    private static RolePermissionItemDto[] MergePermissionSelection(
        IEnumerable<RolePermissionItemDto> permissions,
        ISet<string> selectedPermissionNames)
    {
        return permissions
            .Select(permission => new RolePermissionItemDto
            {
                Name = permission.Name,
                DisplayName = permission.DisplayName,
                Description = permission.Description,
                IsGranted = selectedPermissionNames.Contains(permission.Name)
            })
            .ToArray();
    }
}
