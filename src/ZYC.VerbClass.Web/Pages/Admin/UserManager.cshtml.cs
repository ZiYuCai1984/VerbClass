using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.Departments;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Application.Contracts.Users;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Web.Pages.Admin.UserManagerPartials;

namespace ZYC.VerbClass.Web.Pages.Admin;

[Authorize(VerbClassPermissions.Users.Access)]
public class UserManagerModel : VerbClassPageModel
{
    public UserManagerModel(
        ILifetimeScope lifetimeScope,
        IUserManagementAppService userManagementAppService,
        IDepartmentAppService departmentAppService,
        IUserAvatarAppService userAvatarAppService) : base(lifetimeScope)
    {
        UserManagementAppService = userManagementAppService;
        DepartmentAppService = departmentAppService;
        UserAvatarAppService = userAvatarAppService;
    }

    protected override string PageTitle => "User Manager";

    private IUserManagementAppService UserManagementAppService { get; }

    private IDepartmentAppService DepartmentAppService { get; }

    private IUserAvatarAppService UserAvatarAppService { get; }

    [BindProperty(SupportsGet = true)] public Guid? UserId { get; set; }

    public UserManagerContentModel State { get; private set; } = new();

    private static string RouteKey => nameof(UserId);

    public async Task OnGetAsync()
    {
        State = await BuildUserManagerContentAsync(UserId);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid userId)
    {
        return await BuildUserInfoPartialAsync(userId);
    }

    public async Task<PartialViewResult> OnGetViewAsync(Guid? userId = null)
    {
        SetReplaceUrl(RouteKey, userId);
        return await BuildUserManagerContentPartialAsync(userId);
    }

    public async Task<IActionResult> OnGetCreateAsync(Guid? returnUserId = null)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanCreate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        SetReplaceUrl(RouteKey);
        return await BuildUserManagerContentPartialAsync(
            null,
            BuildCreateEditorModel(returnUserId)
        );
    }

    public async Task<IActionResult> OnGetEditAsync(Guid userId)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        try
        {
            var editor = await BuildEditEditorModelAsync(userId);
            SetReplaceUrl(RouteKey, userId);
            return await BuildUserManagerContentPartialAsync(userId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Edit failed");
            SetReplaceUrl(RouteKey);
            return await BuildUserManagerContentPartialAsync();
        }
    }

    public async Task<IActionResult> OnPostCreateAsync(UserEditorInput input)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanCreate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        var editor = BuildEditorModel(input, true);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey);
            return await BuildUserManagerContentPartialAsync(null, editor);
        }

        try
        {
            var user = await UserManagementAppService.CreateAsync(ToCreateInput(editor));

            ToastInfo($"User {user.DisplayName} was created.", "User created");
            SetReplaceUrl(RouteKey, user.Id);
            return await BuildUserManagerContentPartialAsync(user.Id);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "User operation failed.");
            SetReplaceUrl(RouteKey);
            return await BuildUserManagerContentPartialAsync(null, editor);
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid userId, UserEditorInput input)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        var editor = BuildEditorModel(input, false, userId);
        NormalizeEditor(editor);
        ValidateEditor(editor);

        if (editor.HasErrors)
        {
            SetReplaceUrl(RouteKey, userId);
            return await BuildUserManagerContentPartialAsync(userId, editor);
        }

        try
        {
            var user = await UserManagementAppService.UpdateAsync(userId, ToUpdateInput(editor));

            ToastInfo($"User {user.DisplayName} was updated.", "User saved");
            SetReplaceUrl(RouteKey, userId);
            return await BuildUserManagerContentPartialAsync(userId);
        }
        catch (AbpValidationException ex)
        {
            EditorValidationSupport.ApplyAbpValidationException(editor, ex, "User operation failed.");
            SetReplaceUrl(RouteKey, userId);
            return await BuildUserManagerContentPartialAsync(userId, editor);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Update failed");
            SetReplaceUrl(RouteKey);
            return await BuildUserManagerContentPartialAsync();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid userId)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanDelete)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        try
        {
            var user = await UserManagementAppService.DeleteAsync(userId);

            ToastWarn($"User {user.DisplayName} was deleted.", "User deleted");
            SetReplaceUrl(RouteKey);
            return await BuildUserManagerContentPartialAsync();
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Delete failed");
            SetReplaceUrl(RouteKey, userId);
            return await BuildUserManagerContentPartialAsync(userId);
        }
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync(Guid userId, IFormFile? avatar)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        if (avatar is null || avatar.Length == 0)
        {
            ToastError("Please choose an image file first.", "Upload failed");
            return await BuildUserInfoPartialAsync(userId);
        }

        try
        {
            await using var stream = avatar.OpenReadStream();
            await UserAvatarAppService.UploadAsync(
                userId,
                new RemoteStreamContent(stream, avatar.FileName, avatar.ContentType)
            );

            ToastInfo("Avatar uploaded successfully.", "Upload complete");
            return await BuildUserInfoPartialAsync(userId);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Upload failed");
            return await BuildUserInfoPartialAsync(userId);
        }
    }

    public async Task<IActionResult> OnPostRemoveAvatarAsync(Guid userId)
    {
        if (!(await UserManagementAppService.GetPermissionsAsync()).CanUpdate)
        {
            return ForbidOrHtmxRedirectToAccessDenied();
        }

        try
        {
            await UserAvatarAppService.RemoveAsync(userId);
            ToastWarn("Avatar removed.", "Avatar deleted");
            return await BuildUserInfoPartialAsync(userId);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Remove failed");
            return await BuildUserInfoPartialAsync(userId);
        }
    }

    private async Task<PartialViewResult> BuildUserInfoPartialAsync(Guid userId)
    {
        var model = await BuildUserInfoModelAsync(userId);
        return Partial("~/Pages/Admin/UserManagerPartials/_UserInfo.cshtml", model);
    }

    private async Task<UserInfoModel> BuildUserInfoModelAsync(Guid userId)
    {
        var user = await UserManagementAppService.GetAsync(userId);
        var model = new UserInfoModel
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            UserName = user.UserName,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = user.Roles,
            Departments = user.Departments,
            HasAvatar = user.HasAvatar,
            CanUpdate = user.CanUpdate,
            CanDelete = user.CanDelete
        };

        await LoadAvatarAsync(model);

        return model;
    }

    private async Task<PartialViewResult> BuildUserManagerContentPartialAsync(
        Guid? activeUserId = null,
        UserEditorModel? editor = null)
    {
        var model = await BuildUserManagerContentAsync(activeUserId, editor);
        return Partial("~/Pages/Admin/UserManagerPartials/_UserManagerContent.cshtml", model);
    }

    private async Task<UserManagerContentModel> BuildUserManagerContentAsync(
        Guid? activeUserId = null,
        UserEditorModel? editor = null)
    {
        var permissions = await UserManagementAppService.GetPermissionsAsync();
        var users = (await UserManagementAppService.GetListAsync())
            .Select(user => new UserListItem(
                user.Id,
                user.DisplayName,
                user.UserName,
                user.Email,
                user.IsActive,
                user.DepartmentSummary
            ))
            .ToArray();

        if (activeUserId.HasValue && users.All(x => x.Id != activeUserId.Value))
        {
            activeUserId = null;
        }

        UserInfoModel? selectedUser = null;
        if (editor is null && activeUserId.HasValue)
        {
            selectedUser = await BuildUserInfoModelAsync(activeUserId.Value);
        }

        if (editor is not null)
        {
            editor.CanAssignRoles = permissions.CanAssignRoles;
            await PopulateDepartmentOptionsAsync(editor);
            await PopulateRoleOptionsAsync(editor);
        }

        return new UserManagerContentModel
        {
            Users = users,
            ActiveUserId = activeUserId,
            SelectedUser = selectedUser,
            Editor = editor,
            CanCreate = permissions.CanCreate
        };
    }

    private static UserEditorModel BuildCreateEditorModel(Guid? returnUserId = null)
    {
        return new UserEditorModel
        {
            IsCreate = true,
            IsActive = true,
            ReturnUserId = returnUserId
        };
    }

    private async Task<UserEditorModel> BuildEditEditorModelAsync(Guid userId)
    {
        var user = await UserManagementAppService.GetEditorAsync(userId);

        return new UserEditorModel
        {
            IsCreate = false,
            UserId = user.Id,
            UserName = user.UserName,
            Surname = user.Surname,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DepartmentIds = user.DepartmentIds,
            PrimaryDepartmentId = user.PrimaryDepartmentId,
            RoleNames = user.RoleNames,
            IsActive = user.IsActive,
            ForcePasswordChangeOnNextLogin = user.ForcePasswordChangeOnNextLogin,
            ReturnUserId = user.Id
        };
    }

    private static UserEditorModel BuildEditorModel(
        UserEditorInput input,
        bool isCreate,
        Guid? userId = null)
    {
        return new UserEditorModel
        {
            IsCreate = isCreate,
            UserId = userId,
            UserName = input.UserName,
            Surname = input.Surname,
            Name = input.Name,
            Email = input.Email,
            PhoneNumber = input.PhoneNumber,
            DepartmentIds = input.DepartmentIds,
            PrimaryDepartmentId = input.PrimaryDepartmentId,
            RoleNames = input.RoleNames,
            IsActive = input.IsActive,
            Password = input.Password,
            ConfirmPassword = input.ConfirmPassword,
            ForcePasswordChangeOnNextLogin = input.ForcePasswordChangeOnNextLogin,
            ReturnUserId = input.ReturnUserId
        };
    }

    private static CreateUserInput ToCreateInput(UserEditorModel editor)
    {
        return new CreateUserInput
        {
            UserName = editor.UserName,
            Surname = editor.Surname,
            Name = editor.Name,
            Email = editor.Email,
            PhoneNumber = editor.PhoneNumber,
            DepartmentIds = editor.DepartmentIds,
            PrimaryDepartmentId = editor.PrimaryDepartmentId,
            RoleNames = editor.RoleNames,
            IsActive = editor.IsActive,
            Password = editor.Password ?? string.Empty,
            ConfirmPassword = editor.ConfirmPassword ?? string.Empty,
            ForcePasswordChangeOnNextLogin = editor.ForcePasswordChangeOnNextLogin
        };
    }

    private static UpdateUserInput ToUpdateInput(UserEditorModel editor)
    {
        return new UpdateUserInput
        {
            UserName = editor.UserName,
            Surname = editor.Surname,
            Name = editor.Name,
            Email = editor.Email,
            PhoneNumber = editor.PhoneNumber,
            DepartmentIds = editor.DepartmentIds,
            PrimaryDepartmentId = editor.PrimaryDepartmentId,
            RoleNames = editor.RoleNames,
            IsActive = editor.IsActive,
            Password = editor.Password,
            ConfirmPassword = editor.ConfirmPassword,
            ForcePasswordChangeOnNextLogin = editor.ForcePasswordChangeOnNextLogin
        };
    }

    private static void NormalizeEditor(UserEditorModel editor)
    {
        editor.UserName = editor.UserName.Trim();
        editor.Surname = editor.Surname.Trim();
        editor.Name = editor.Name.Trim();
        editor.Email = editor.Email.Trim();
        editor.PhoneNumber = string.IsNullOrWhiteSpace(editor.PhoneNumber)
            ? null
            : editor.PhoneNumber.Trim();
        editor.DepartmentIds = editor.DepartmentIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();
        editor.RoleNames = editor.RoleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (editor.PrimaryDepartmentId == Guid.Empty)
        {
            editor.PrimaryDepartmentId = null;
        }
    }

    private static void ValidateEditor(UserEditorModel editor)
    {
        EditorValidationSupport.ApplyDataAnnotations(editor);
    }

    private async Task PopulateDepartmentOptionsAsync(UserEditorModel editor)
    {
        editor.DepartmentOptions.Clear();

        foreach (var department in await DepartmentAppService.GetUserDepartmentOptionsAsync(editor.DepartmentIds))
        {
            editor.DepartmentOptions.Add(new UserDepartmentOption(department.Id, department.Label));
        }
    }

    private async Task PopulateRoleOptionsAsync(UserEditorModel editor)
    {
        editor.RoleOptions.Clear();

        if (!editor.CanAssignRoles)
        {
            return;
        }

        foreach (var roleName in await UserManagementAppService.GetAssignableRoleNamesAsync())
        {
            editor.RoleOptions.Add(roleName);
        }
    }

    private async Task LoadAvatarAsync(UserInfoModel model)
    {
        try
        {
            var avatar = await UserAvatarAppService.GetAsync(model.Id);
            if (avatar is null)
            {
                model.AvatarImageSource = null;
                return;
            }

            model.AvatarImageSource = await ToDataUrlAsync(avatar);
        }
        catch (UserFriendlyException ex)
        {
            ToastError(ex.Message, "Avatar load failed");
            model.AvatarImageSource = null;
        }
        catch (Exception)
        {
            ToastError("Avatar could not be loaded.", "Avatar load failed");
            model.AvatarImageSource = null;
        }
    }

    private static async Task<string> ToDataUrlAsync(IRemoteStreamContent avatar)
    {
        await using var stream = avatar.GetStream();
        await using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);

        var contentType = string.IsNullOrWhiteSpace(avatar.ContentType)
            ? "image/png"
            : avatar.ContentType;

        return $"data:{contentType};base64,{Convert.ToBase64String(buffer.ToArray())}";
    }
}
