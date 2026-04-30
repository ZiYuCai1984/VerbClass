using Riok.Mapperly.Abstractions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using ZYC.VerbClass.Application.Contracts.Departments;
using ZYC.VerbClass.Application.Contracts.Roles;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Application.Contracts.Users;
using ZYC.VerbClass.Application.IdentityUsers;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.Departments;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.Application;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class VerbClassApplicationMappers
{
    public static partial DepartmentEditorDto ToDepartmentEditorDto(Department department);

    public static partial DepartmentCommandResultDto ToDepartmentCommandResultDto(Department department);

    public static DepartmentDetailDto ToDepartmentDetailDto(
        Department department,
        string? parentDepartmentName,
        string pathDisplay,
        int currentUserCount,
        bool hasChildren,
        bool canCreateChild,
        bool canUpdate,
        bool canDelete)
    {
        return new DepartmentDetailDto
        {
            Id = department.Id,
            Code = department.Code,
            Name = department.Name,
            ShortName = department.ShortName,
            ParentDepartmentName = parentDepartmentName,
            PathDisplay = pathDisplay,
            Sort = department.Sort,
            IsActive = department.IsActive,
            CanAssignUsers = department.CanAssignUsers,
            CurrentUserCount = currentUserCount,
            EffectiveFrom = department.EffectiveFrom,
            EffectiveTo = department.EffectiveTo,
            CreationTime = department.CreationTime,
            LastModificationTime = department.LastModificationTime,
            HasChildren = hasChildren,
            CanCreateChild = canCreateChild,
            CanUpdate = canUpdate,
            CanDelete = canDelete
        };
    }

    public static DepartmentListItemDto ToDepartmentListItemDto(
        Department department,
        string pathDisplay,
        int depth,
        int currentUserCount)
    {
        return new DepartmentListItemDto
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            ShortName = department.ShortName,
            PathDisplay = pathDisplay,
            Depth = depth,
            IsActive = department.IsActive,
            CanAssignUsers = department.CanAssignUsers,
            CurrentUserCount = currentUserCount
        };
    }

    public static DepartmentParentOptionDto ToDepartmentParentOptionDto(Guid? id, string label)
    {
        return new DepartmentParentOptionDto
        {
            Id = id,
            Label = label
        };
    }

    public static DepartmentPermissionsDto ToDepartmentPermissionsDto(
        bool canCreate,
        bool canUpdate,
        bool canDelete)
    {
        return new DepartmentPermissionsDto
        {
            CanCreate = canCreate,
            CanUpdate = canUpdate,
            CanDelete = canDelete
        };
    }

    public static UserDepartmentDisplayItemDto ToUserDepartmentDisplayItemDto(
        DepartmentAssignment assignment,
        string path)
    {
        return new UserDepartmentDisplayItemDto
        {
            DepartmentId = assignment.DepartmentId,
            Path = path,
            IsPrimary = assignment.IsPrimary
        };
    }

    public static UserDepartmentOptionDto ToUserDepartmentOptionDto(Guid id, string label)
    {
        return new UserDepartmentOptionDto
        {
            Id = id,
            Label = label
        };
    }

    public static UserDepartmentSummaryDto ToUserDepartmentSummaryDto(Guid userId, string? summary)
    {
        return new UserDepartmentSummaryDto
        {
            UserId = userId,
            Summary = summary
        };
    }

    public static UserListItemDto ToUserListItemDto(IdentityUser user, string? departmentSummary)
    {
        return new UserListItemDto
        {
            Id = user.Id,
            DisplayName = IdentityUserDisplayNameSupport.BuildDisplayName(user),
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            DepartmentSummary = departmentSummary
        };
    }

    public static UserDetailDto ToUserDetailDto(
        IdentityUser user,
        string[] roles,
        UserDepartmentDisplayItemDto[] departments,
        bool hasAvatar,
        bool canUpdate,
        bool canDelete)
    {
        return new UserDetailDto
        {
            Id = user.Id,
            DisplayName = IdentityUserDisplayNameSupport.BuildDisplayName(user),
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = roles,
            Departments = departments,
            HasAvatar = hasAvatar,
            CanUpdate = canUpdate,
            CanDelete = canDelete
        };
    }

    public static UserEditorDto ToUserEditorDto(
        IdentityUser user,
        DepartmentAssignment[] currentAssignments,
        string[] roleNames)
    {
        var primaryDepartmentId = currentAssignments
            .FirstOrDefault(x => x.IsPrimary)
            ?.DepartmentId;

        return new UserEditorDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Surname = user.Surname ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            DepartmentIds = currentAssignments.Select(x => x.DepartmentId).ToArray(),
            PrimaryDepartmentId = primaryDepartmentId ?? currentAssignments.FirstOrDefault()?.DepartmentId,
            RoleNames = roleNames,
            IsActive = user.IsActive,
            ForcePasswordChangeOnNextLogin = user.ShouldChangePasswordOnNextLogin
        };
    }

    public static UserCommandResultDto ToUserCommandResultDto(IdentityUser user)
    {
        return new UserCommandResultDto
        {
            Id = user.Id,
            DisplayName = IdentityUserDisplayNameSupport.BuildDisplayName(user)
        };
    }

    public static UserPermissionsDto ToUserPermissionsDto(
        bool canCreate,
        bool canUpdate,
        bool canDelete,
        bool canAssignRoles)
    {
        return new UserPermissionsDto
        {
            CanCreate = canCreate,
            CanUpdate = canUpdate,
            CanDelete = canDelete,
            CanAssignRoles = canAssignRoles
        };
    }

    public static UserProfileDto ToUserProfileDto(
        IdentityUser user,
        UserProfile? profile,
        string[] roles)
    {
        return new UserProfileDto
        {
            DisplayName = IdentityUserDisplayNameSupport.BuildDisplayName(user, UserProfileDto.DefaultDisplayName),
            UserNameDisplay = user.UserName ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            HasCustomAvatar = profile?.AvatarFileId.HasValue == true,
            AvatarVersion = profile?.AvatarFileId?.ToString("N") ?? UserProfileDto.DefaultAvatarVersion,
            Roles = roles
        };
    }

    public static UserSettingsDto ToUserSettingsDto(
        IdentityUser user,
        UserProfile? profile,
        string[] roles,
        UpdateUserSettingsInput input)
    {
        return new UserSettingsDto
        {
            DisplayName = IdentityUserDisplayNameSupport.BuildDisplayName(user, UserSettingsDto.DefaultDisplayName),
            UserNameDisplay = user.UserName ?? string.Empty,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            HasCustomAvatar = profile?.AvatarFileId.HasValue == true,
            AvatarVersion = profile?.AvatarFileId?.ToString("N") ?? UserSettingsDto.DefaultAvatarVersion,
            Roles = roles,
            Input = input
        };
    }

    public static UpdateUserSettingsInput ToUpdateUserSettingsInput(IdentityUser user, UserProfile? profile)
    {
        return new UpdateUserSettingsInput
        {
            UserName = user.UserName ?? string.Empty,
            Surname = user.Surname ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            SurnameKanji = profile?.NameInfo.SurnameKanji,
            NameKanji = profile?.NameInfo.NameKanji,
            SurnameKana = profile?.NameInfo.SurnameKana,
            NameKana = profile?.NameInfo.NameKana,
            SurnameRomanized = profile?.NameInfo.SurnameRomanized,
            NameRomanized = profile?.NameInfo.NameRomanized,
            BirthDate = profile?.BirthDate,
            Gender = profile?.Gender,
            BloodType = profile?.BloodType,
            Nationality = profile?.Nationality,
            Country = profile?.AddressInfo.Country,
            Prefecture = profile?.AddressInfo.Prefecture,
            City = profile?.AddressInfo.City,
            Street = profile?.AddressInfo.Street,
            PostalCode = profile?.AddressInfo.PostalCode,
            EnrollmentYear = profile?.EnrollmentYear
        };
    }

    public static RolePermissionsDto ToRolePermissionsDto(bool canManagePermissions)
    {
        return new RolePermissionsDto
        {
            CanManagePermissions = canManagePermissions
        };
    }

    public static RoleListItemDto ToRoleListItemDto(
        string roleName,
        int userCount,
        int grantedPermissionCount,
        int availablePermissionCount)
    {
        return new RoleListItemDto
        {
            Name = roleName,
            UserCount = userCount,
            GrantedPermissionCount = grantedPermissionCount,
            AvailablePermissionCount = availablePermissionCount
        };
    }

    public static RoleDetailDto ToRoleDetailDto(
        string roleName,
        int userCount,
        int grantedPermissionCount,
        int availablePermissionCount,
        RolePermissionEntryDto[] grantedPermissions,
        RoleAssignedUserDto[] assignedUsers,
        int remainingUserCount,
        bool canManagePermissions)
    {
        return new RoleDetailDto
        {
            Name = roleName,
            UserCount = userCount,
            GrantedPermissionCount = grantedPermissionCount,
            AvailablePermissionCount = availablePermissionCount,
            GrantedPermissions = grantedPermissions,
            AssignedUsers = assignedUsers,
            RemainingUserCount = remainingUserCount,
            CanManagePermissions = canManagePermissions
        };
    }

    public static RolePermissionEditorDto ToRolePermissionEditorDto(
        string roleName,
        RolePermissionItemDto[] permissions)
    {
        return new RolePermissionEditorDto
        {
            Name = roleName,
            Permissions = permissions
        };
    }

    public static RoleCommandResultDto ToRoleCommandResultDto(
        string roleName,
        int grantedPermissionCount,
        int availablePermissionCount)
    {
        return new RoleCommandResultDto
        {
            Name = roleName,
            GrantedPermissionCount = grantedPermissionCount,
            AvailablePermissionCount = availablePermissionCount
        };
    }

    public static RoleAssignedUserDto ToRoleAssignedUserDto(IdentityUser user)
    {
        return new RoleAssignedUserDto
        {
            Id = user.Id,
            DisplayName = IdentityUserDisplayNameSupport.BuildDisplayName(user),
            UserName = user.UserName ?? string.Empty
        };
    }

    public static RolePermissionItemDto ToRolePermissionItemDto(
        PermissionDefinition permission,
        string displayName,
        bool isGranted)
    {
        return new RolePermissionItemDto
        {
            Name = permission.Name,
            DisplayName = displayName,
            Description = permission.Name,
            IsGranted = isGranted
        };
    }

    public static RolePermissionEntryDto ToRolePermissionEntryDto(
        PermissionDefinition permission,
        string displayName)
    {
        return new RolePermissionEntryDto
        {
            Name = permission.Name,
            DisplayName = displayName
        };
    }
}
