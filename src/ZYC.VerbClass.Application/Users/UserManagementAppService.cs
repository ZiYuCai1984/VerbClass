using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.Departments;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Application.Contracts.Users;
using ZYC.VerbClass.Application.Departments;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.Departments;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.Application.Users;

[Authorize(VerbClassPermissions.Users.Access)]
public class UserManagementAppService : VerbClassAppService, IUserManagementAppService
{
    private readonly IRepository<DepartmentAssignment, Guid> _departmentAssignmentRepository;
    private readonly IRepository<Department, Guid> _departmentRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IUserAvatarAppService _userAvatarAppService;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<UserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public UserManagementAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<DepartmentAssignment, Guid> departmentAssignmentRepository,
        IRepository<Department, Guid> departmentRepository,
        IRepository<UserProfile, Guid> userProfileRepository,
        IUserAvatarAppService userAvatarAppService,
        IdentityUserManager userManager,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _departmentAssignmentRepository = departmentAssignmentRepository;
        _departmentRepository = departmentRepository;
        _userProfileRepository = userProfileRepository;
        _userAvatarAppService = userAvatarAppService;
        _userManager = userManager;
        _guidGenerator = guidGenerator;
    }

    public async Task<UserPermissionsDto> GetPermissionsAsync()
    {
        return await GetPermissionsInternalAsync();
    }

    public async Task<UserListItemDto[]> GetListAsync()
    {
        var users = await _userRepository.GetListAsync();
        if (users.Count == 0)
        {
            return [];
        }

        var departmentSummariesByUserId =
            await BuildDepartmentSummariesByUserIdAsync(users.Select(x => x.Id).ToArray());

        return users
            .Select(user => VerbClassApplicationMappers.ToUserListItemDto(
                user,
                departmentSummariesByUserId.GetValueOrDefault(user.Id)))
            .ToArray();
    }

    public async Task<UserDetailDto> GetAsync(Guid userId)
    {
        var user = await _userRepository.FindAsync(userId)
                   ?? throw CreateUserNotFoundException();
        var permissions = await GetPermissionsInternalAsync();
        var roles = await _userManager.GetRolesAsync(user);
        var departments = await BuildDepartmentDisplayItemsAsync(userId);
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);

        return VerbClassApplicationMappers.ToUserDetailDto(
            user,
            roles.ToArray(),
            departments,
            profile?.AvatarFileId.HasValue == true,
            permissions.CanUpdate,
            permissions.CanDelete && CurrentUser.Id != userId
        );
    }

    [Authorize(VerbClassPermissions.Users.Update)]
    public async Task<UserEditorDto> GetEditorAsync(Guid userId)
    {
        var user = await _userRepository.FindAsync(userId)
                   ?? throw CreateUserNotFoundException();
        var currentAssignments = await GetCurrentDepartmentAssignmentsAsync(userId);
        var roleNames = (await _userManager.GetRolesAsync(user))
            .Where(VerbClassRoles.IsManagedRole)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return VerbClassApplicationMappers.ToUserEditorDto(user, currentAssignments, roleNames);
    }

    [Authorize(VerbClassPermissions.Users.AssignRoles)]
    public async Task<string[]> GetAssignableRoleNamesAsync()
    {
        return await GetManageableRoleNamesAsync();
    }

    [Authorize(VerbClassPermissions.Users.Create)]
    public async Task<UserCommandResultDto> CreateAsync(CreateUserInput input)
    {
        NormalizeInput(input);
        await ValidateCreateInputAsync(input);

        var canAssignRoles = await CanAssignRolesAsync();
        EnsureCanAssignRoles(canAssignRoles, input.RoleNames);

        var user = new IdentityUser(
            _guidGenerator.Create(),
            input.UserName,
            input.Email,
            CurrentTenant.Id
        )
        {
            Surname = input.Surname,
            Name = input.Name
        };
        user.SetPhoneNumber(input.PhoneNumber ?? string.Empty, false);
        user.SetIsActive(input.IsActive);

        var createResult = await _userManager.CreateAsync(user, input.Password);
        if (!createResult.Succeeded)
        {
            throw CreateValidationException(createResult, "User could not be created.");
        }

        user.SetShouldChangePasswordOnNextLogin(input.ForcePasswordChangeOnNextLogin);
        var passwordFlagResult = await _userManager.UpdateAsync(user);
        if (!passwordFlagResult.Succeeded)
        {
            throw CreateValidationException(passwordFlagResult, "User could not be created.");
        }

        await SyncDepartmentAssignmentsAsync(user.Id, input.DepartmentIds, input.PrimaryDepartmentId);

        if (canAssignRoles)
        {
            await SyncUserRolesAsync(user, input.RoleNames);
        }

        return VerbClassApplicationMappers.ToUserCommandResultDto(user);
    }

    [Authorize(VerbClassPermissions.Users.Update)]
    public async Task<UserCommandResultDto> UpdateAsync(Guid userId, UpdateUserInput input)
    {
        NormalizeInput(input);
        await ValidateUpdateInputAsync(input);

        var canAssignRoles = await CanAssignRolesAsync();
        EnsureCanAssignRoles(canAssignRoles, input.RoleNames);

        var user = await _userRepository.FindAsync(userId)
                   ?? throw CreateUserNotFoundException();

        user.SetUserNameWithoutValidation(input.UserName, NormalizeLookupValue(input.UserName));
        user.SetEmailWithoutValidation(input.Email, NormalizeLookupValue(input.Email));
        user.Surname = input.Surname;
        user.Name = input.Name;
        user.SetPhoneNumber(
            input.PhoneNumber ?? string.Empty,
            string.Equals(user.PhoneNumber, input.PhoneNumber, StringComparison.Ordinal) &&
            user.PhoneNumberConfirmed
        );
        user.SetIsActive(input.IsActive);
        user.SetShouldChangePasswordOnNextLogin(input.ForcePasswordChangeOnNextLogin);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw CreateValidationException(updateResult, "User could not be updated.");
        }

        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, input.Password);
            if (!resetResult.Succeeded)
            {
                throw CreateValidationException(resetResult, "User could not be updated.");
            }
        }

        await SyncDepartmentAssignmentsAsync(userId, input.DepartmentIds, input.PrimaryDepartmentId);

        if (canAssignRoles)
        {
            await SyncUserRolesAsync(user, input.RoleNames);
        }

        return VerbClassApplicationMappers.ToUserCommandResultDto(user);
    }

    [Authorize(VerbClassPermissions.Users.Delete)]
    public async Task<UserCommandResultDto> DeleteAsync(Guid userId)
    {
        if (CurrentUser.Id == userId)
        {
            throw new UserFriendlyException("You cannot delete the current signed-in user.");
        }

        var user = await _userRepository.FindAsync(userId)
                   ?? throw CreateUserNotFoundException();

        await _userAvatarAppService.RemoveAsync(userId);

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            throw new UserFriendlyException(string.Join("; ", deleteResult.Errors.Select(x => x.Description)));
        }

        return VerbClassApplicationMappers.ToUserCommandResultDto(user);
    }

    private async Task<Dictionary<Guid, string?>> BuildDepartmentSummariesByUserIdAsync(Guid[] userIds)
    {
        var normalizedUserIds = userIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedUserIds.Length == 0)
        {
            return [];
        }

        var assignments = await _departmentAssignmentRepository.GetListAsync(x => normalizedUserIds.Contains(x.UserId));
        if (assignments.Count == 0)
        {
            return [];
        }

        var departments = await _departmentRepository.GetListAsync();
        var departmentsById = departments.ToDictionary(x => x.Id);
        var effectiveDate = DateTime.UtcNow.Date;

        return assignments
            .GroupBy(x => x.UserId)
            .ToDictionary(
                group => group.Key,
                group => DepartmentAppServiceSupport.BuildDepartmentSummary(
                    DepartmentAppServiceSupport.BuildDepartmentDisplayItems(
                        group,
                        effectiveDate,
                        departmentsById
                    )
                )
            );
    }

    private async Task<UserDepartmentDisplayItemDto[]> BuildDepartmentDisplayItemsAsync(Guid userId)
    {
        var assignments = await _departmentAssignmentRepository.GetListAsync(x => x.UserId == userId);
        if (assignments.Count == 0)
        {
            return [];
        }

        var departments = await _departmentRepository.GetListAsync();
        var departmentsById = departments.ToDictionary(x => x.Id);
        return DepartmentAppServiceSupport.BuildDepartmentDisplayItems(
            assignments,
            DateTime.UtcNow.Date,
            departmentsById
        );
    }

    private async Task<DepartmentAssignment[]> GetCurrentDepartmentAssignmentsAsync(Guid userId)
    {
        var assignments = await _departmentAssignmentRepository.GetListAsync(x => x.UserId == userId);
        return SelectCurrentAssignments(assignments, DateTime.UtcNow.Date);
    }

    private static DepartmentAssignment[] SelectCurrentAssignments(
        IEnumerable<DepartmentAssignment> assignments,
        DateTime effectiveDate)
    {
        return assignments
            .Where(x => x.EffectiveFrom.Date <= effectiveDate &&
                        (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= effectiveDate))
            .GroupBy(x => x.DepartmentId)
            .Select(group => group
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveFrom)
                .ThenBy(x => x.Id)
                .First())
            .ToArray();
    }

    private async Task ValidateCreateInputAsync(CreateUserInput input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (string.IsNullOrWhiteSpace(input.Password))
        {
            validationErrors.Add(new ValidationResult(
                "Initial password is required.",
                [nameof(CreateUserInput.Password)]
            ));
        }

        if (string.IsNullOrWhiteSpace(input.ConfirmPassword))
        {
            validationErrors.Add(new ValidationResult(
                "Confirm password is required.",
                [nameof(CreateUserInput.ConfirmPassword)]
            ));
        }
        else if (!string.Equals(input.Password, input.ConfirmPassword, StringComparison.Ordinal))
        {
            validationErrors.Add(new ValidationResult(
                "Passwords do not match.",
                [nameof(CreateUserInput.ConfirmPassword)]
            ));
        }

        await ValidateSharedInputAsync(input, validationErrors);
    }

    private async Task ValidateUpdateInputAsync(UpdateUserInput input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        var requiresPasswordReset = !string.IsNullOrWhiteSpace(input.Password) ||
                                    !string.IsNullOrWhiteSpace(input.ConfirmPassword);

        if (requiresPasswordReset)
        {
            if (string.IsNullOrWhiteSpace(input.Password))
            {
                validationErrors.Add(new ValidationResult(
                    "Reset password is required.",
                    [nameof(UpdateUserInput.Password)]
                ));
            }

            if (string.IsNullOrWhiteSpace(input.ConfirmPassword))
            {
                validationErrors.Add(new ValidationResult(
                    "Confirm password is required.",
                    [nameof(UpdateUserInput.ConfirmPassword)]
                ));
            }
            else if (!string.Equals(input.Password, input.ConfirmPassword, StringComparison.Ordinal))
            {
                validationErrors.Add(new ValidationResult(
                    "Passwords do not match.",
                    [nameof(UpdateUserInput.ConfirmPassword)]
                ));
            }
        }

        await ValidateSharedInputAsync(input, validationErrors);
    }

    private async Task ValidateSharedInputAsync(
        UserInputBase input,
        IList<ValidationResult> validationErrors)
    {
        await ValidateDepartmentSelectionAsync(input, validationErrors);
        await ValidateRoleSelectionAsync(input, validationErrors);

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("User input is invalid.", validationErrors);
        }
    }

    private async Task ValidateDepartmentSelectionAsync(
        UserInputBase input,
        IList<ValidationResult> validationErrors)
    {
        if (input.DepartmentIds.Length == 0)
        {
            if (input.PrimaryDepartmentId.HasValue)
            {
                validationErrors.Add(new ValidationResult(
                    "Primary department requires at least one selected department.",
                    [nameof(UserInputBase.PrimaryDepartmentId)]
                ));
            }

            return;
        }

        if (!input.PrimaryDepartmentId.HasValue)
        {
            validationErrors.Add(new ValidationResult(
                "Choose a primary department.",
                [nameof(UserInputBase.PrimaryDepartmentId)]
            ));
        }
        else if (!input.DepartmentIds.Contains(input.PrimaryDepartmentId.Value))
        {
            validationErrors.Add(new ValidationResult(
                "Primary department must be one of the selected departments.",
                [nameof(UserInputBase.PrimaryDepartmentId)]
            ));
        }

        var existingDepartmentIds = (await _departmentRepository.GetListAsync(x => input.DepartmentIds.Contains(x.Id)))
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var departmentId in input.DepartmentIds)
        {
            if (!existingDepartmentIds.Contains(departmentId))
            {
                validationErrors.Add(new ValidationResult(
                    "One or more departments were not found.",
                    [nameof(UserInputBase.DepartmentIds)]
                ));
                break;
            }
        }
    }

    private async Task ValidateRoleSelectionAsync(
        UserInputBase input,
        IList<ValidationResult> validationErrors)
    {
        if (input.RoleNames.Length == 0)
        {
            return;
        }

        var requestedRoles = input.RoleNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var manageableRoles = (await GetManageableRoleNamesAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in requestedRoles)
        {
            if (!manageableRoles.Contains(roleName))
            {
                validationErrors.Add(new ValidationResult(
                    "One or more roles are not available.",
                    [nameof(UserInputBase.RoleNames)]
                ));
                break;
            }
        }
    }

    private async Task SyncDepartmentAssignmentsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> departmentIds,
        Guid? primaryDepartmentId)
    {
        var assignments = await _departmentAssignmentRepository.GetListAsync(x => x.UserId == userId);
        var effectiveFrom = DateTime.UtcNow.Date;
        var selectedDepartmentIds = departmentIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();
        var selectedDepartmentIdSet = selectedDepartmentIds.ToHashSet();
        var currentAssignmentsByDepartmentId = assignments
            .Where(x => x.EffectiveFrom.Date <= effectiveFrom &&
                        (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= effectiveFrom))
            .GroupBy(x => x.DepartmentId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(item => item.IsPrimary)
                    .ThenByDescending(item => item.EffectiveFrom)
                    .ThenBy(item => item.Id)
                    .ToArray());
        var effectiveTo = effectiveFrom.AddDays(-1);

        async Task RemoveCurrentAssignmentAsync(DepartmentAssignment assignment)
        {
            if (assignment.EffectiveFrom.Date < effectiveFrom)
            {
                assignment.Close(effectiveTo);
                await _departmentAssignmentRepository.UpdateAsync(assignment, true);
                return;
            }

            await _departmentAssignmentRepository.DeleteAsync(assignment, true);
        }

        foreach (var assignmentGroup in currentAssignmentsByDepartmentId.Values)
        {
            foreach (var duplicateAssignment in assignmentGroup.Skip(1))
            {
                await RemoveCurrentAssignmentAsync(duplicateAssignment);
            }
        }

        foreach (var assignmentGroup in currentAssignmentsByDepartmentId
                     .Where(x => !selectedDepartmentIdSet.Contains(x.Key))
                     .Select(x => x.Value))
        {
            await RemoveCurrentAssignmentAsync(assignmentGroup[0]);
        }

        if (selectedDepartmentIds.Length == 0)
        {
            return;
        }

        var tenantId = CurrentTenant.Id
                       ?? throw new UserFriendlyException("Department assignment requires a tenant context.");
        var normalizedPrimaryDepartmentId = primaryDepartmentId ?? selectedDepartmentIds[0];

        foreach (var departmentId in selectedDepartmentIds)
        {
            if (!currentAssignmentsByDepartmentId.TryGetValue(departmentId, out var currentAssignmentsForDepartment) ||
                currentAssignmentsForDepartment.Length == 0)
            {
                var newAssignment = new DepartmentAssignment(
                    _guidGenerator.Create(),
                    tenantId,
                    userId,
                    departmentId,
                    normalizedPrimaryDepartmentId == departmentId,
                    effectiveFrom
                );

                await _departmentAssignmentRepository.InsertAsync(newAssignment, true);
                continue;
            }

            var currentAssignment = currentAssignmentsForDepartment[0];
            var changed = false;
            var shouldBePrimary = normalizedPrimaryDepartmentId == departmentId;

            if (currentAssignment.IsPrimary != shouldBePrimary)
            {
                currentAssignment.SetPrimary(shouldBePrimary);
                changed = true;
            }

            if (currentAssignment.EffectiveTo.HasValue)
            {
                currentAssignment.ChangePeriod(currentAssignment.EffectiveFrom.Date, null);
                changed = true;
            }

            if (changed)
            {
                await _departmentAssignmentRepository.UpdateAsync(currentAssignment, true);
            }
        }
    }

    private async Task SyncUserRolesAsync(
        IdentityUser user,
        IReadOnlyCollection<string> roleNames)
    {
        var manageableRoles = VerbClassRoles.All.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetRoles = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(manageableRoles.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentRoles = (await _userManager.GetRolesAsync(user))
            .Where(manageableRoles.Contains)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var rolesToRemove = currentRoles
            .Where(x => !targetRoles.Contains(x))
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                throw CreateValidationException(removeResult, "User roles could not be updated.");
            }
        }

        foreach (var roleName in targetRoles.Where(x => !currentRoles.Contains(x)))
        {
            var addResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                throw CreateValidationException(addResult, "User roles could not be updated.");
            }
        }
    }

    private async Task<string[]> GetManageableRoleNamesAsync()
    {
        var availableRoleNames = (await _roleRepository.GetListAsync())
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return VerbClassRoles.All
            .Where(availableRoleNames.Contains)
            .ToArray();
    }

    private async Task<UserPermissionsDto> GetPermissionsInternalAsync()
    {
        return VerbClassApplicationMappers.ToUserPermissionsDto(
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.Create),
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.Update),
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.Delete),
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.AssignRoles)
        );
    }

    private async Task<bool> CanAssignRolesAsync()
    {
        return await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.AssignRoles);
    }

    private static void NormalizeInput(UserInputBase input)
    {
        input.UserName = input.UserName.Trim();
        input.Surname = input.Surname.Trim();
        input.Name = input.Name.Trim();
        input.Email = input.Email.Trim();
        input.PhoneNumber = string.IsNullOrWhiteSpace(input.PhoneNumber)
            ? null
            : input.PhoneNumber.Trim();
        input.DepartmentIds = input.DepartmentIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();
        input.RoleNames = input.RoleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (input.PrimaryDepartmentId == Guid.Empty)
        {
            input.PrimaryDepartmentId = null;
        }
    }

    private static void EnsureCanAssignRoles(bool canAssignRoles, IReadOnlyCollection<string> roleNames)
    {
        if (canAssignRoles || roleNames.Count == 0)
        {
            return;
        }

        throw new AbpAuthorizationException();
    }

    private static AbpValidationException CreateValidationException(IdentityResult result, string message)
    {
        var validationErrors = result.Errors
            .Select(error =>
            {
                var fieldName = error.Code switch
                {
                    "DuplicateUserName" or "InvalidUserName" => nameof(UserInputBase.UserName),
                    "DuplicateEmail" or "InvalidEmail" => nameof(UserInputBase.Email),
                    var passwordCode when passwordCode.StartsWith("Password", StringComparison.OrdinalIgnoreCase) =>
                        nameof(UpdateUserInput.Password),
                    _ => null
                };

                return fieldName is null
                    ? new ValidationResult(error.Description)
                    : new ValidationResult(error.Description, [fieldName]);
            })
            .ToList();

        return new AbpValidationException(message, validationErrors);
    }

    private static string NormalizeLookupValue(string value)
    {
        return value.ToUpperInvariant();
    }

    private static UserFriendlyException CreateUserNotFoundException()
    {
        return new UserFriendlyException("User was not found.");
    }
}