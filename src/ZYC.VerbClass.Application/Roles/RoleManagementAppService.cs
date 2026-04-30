using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.Roles;
using ZYC.VerbClass.Application.IdentityUsers;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Application.Roles;

[Authorize(VerbClassPermissions.Roles.Access)]
public class RoleManagementAppService : VerbClassAppService, IRoleManagementAppService
{
    private const int AssignedUserPreviewLimit = 12;

    private static readonly Regex PermissionLabelWordBoundaryRegex =
        new("([a-z0-9])([A-Z])", RegexOptions.Compiled);

    private readonly IGuidGenerator _guidGenerator;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;

    public RoleManagementAppService(
        IGuidGenerator guidGenerator,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionDefinitionManager permissionDefinitionManager,
        IPermissionGrantRepository permissionGrantRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager)
    {
        _guidGenerator = guidGenerator;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionDefinitionManager = permissionDefinitionManager;
        _permissionGrantRepository = permissionGrantRepository;
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<RolePermissionsDto> GetPermissionsAsync()
    {
        return VerbClassApplicationMappers.ToRolePermissionsDto(
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Roles.ManagePermissions)
        );
    }

    public async Task<RoleListItemDto[]> GetListAsync()
    {
        await EnsureManagedRolesExistAsync();

        var roleRecords = await GetManagedRoleRecordsAsync();
        var userCountByRoleName = await GetUserCountByRoleNameAsync(roleRecords);
        var permissionDefinitions = await GetManageablePermissionDefinitionsAsync();
        var permissionCountByRoleName = await GetGrantedPermissionCountByRoleNameAsync(permissionDefinitions.Keys.ToHashSet(StringComparer.Ordinal));

        return roleRecords
            .Select(role =>
            {
                var roleName = role.Name ?? string.Empty;
                return VerbClassApplicationMappers.ToRoleListItemDto(
                    roleName,
                    userCountByRoleName.GetValueOrDefault(roleName),
                    permissionCountByRoleName.GetValueOrDefault(roleName),
                    permissionDefinitions.Count
                );
            })
            .ToArray();
    }

    public async Task<RoleDetailDto> GetAsync(string roleName)
    {
        var normalizedRoleName = NormalizeManagedRoleName(roleName);
        await EnsureManagedRolesExistAsync();

        await GetManagedRoleAsync(normalizedRoleName);
        var permissionDefinitions = await GetManageablePermissionDefinitionsAsync();
        var grantedPermissionNames = await GetGrantedPermissionNamesAsync(normalizedRoleName, permissionDefinitions.Keys.ToHashSet(StringComparer.Ordinal));
        var users = await GetUsersInRoleAsync(normalizedRoleName);
        var permissions = await GetPermissionsAsync();

        var grantedPermissions = permissionDefinitions.Values
                .Where(permission => grantedPermissionNames.Contains(permission.Name))
                .Select(BuildPermissionEntry)
                .ToArray();
        var assignedUsers = users
                .Take(AssignedUserPreviewLimit)
                .Select(VerbClassApplicationMappers.ToRoleAssignedUserDto)
                .ToArray();

        return VerbClassApplicationMappers.ToRoleDetailDto(
            normalizedRoleName,
            users.Length,
            grantedPermissionNames.Count,
            permissionDefinitions.Count,
            grantedPermissions,
            assignedUsers,
            Math.Max(0, users.Length - AssignedUserPreviewLimit),
            permissions.CanManagePermissions
        );
    }

    [Authorize(VerbClassPermissions.Roles.ManagePermissions)]
    public async Task<RolePermissionEditorDto> GetEditorAsync(string roleName)
    {
        var normalizedRoleName = NormalizeManagedRoleName(roleName);
        await EnsureManagedRolesExistAsync();
        await GetManagedRoleAsync(normalizedRoleName);

        var permissionDefinitions = await GetManageablePermissionDefinitionsAsync();
        var grantedPermissionNames = await GetGrantedPermissionNamesAsync(normalizedRoleName, permissionDefinitions.Keys.ToHashSet(StringComparer.Ordinal));

        return VerbClassApplicationMappers.ToRolePermissionEditorDto(
            normalizedRoleName,
            BuildPermissionItems(permissionDefinitions, grantedPermissionNames)
        );
    }

    [Authorize(VerbClassPermissions.Roles.ManagePermissions)]
    public async Task<RoleCommandResultDto> UpdatePermissionsAsync(string roleName, UpdateRolePermissionsInput input)
    {
        var normalizedRoleName = NormalizeManagedRoleName(roleName);
        await EnsureManagedRolesExistAsync();
        await GetManagedRoleAsync(normalizedRoleName);

        NormalizeInput(input);

        var permissionDefinitions = await GetManageablePermissionDefinitionsAsync();
        var validationErrors = ValidatePermissionNames(input.GrantedPermissionNames, permissionDefinitions);
        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Role permissions are invalid.", validationErrors);
        }

        var grantedPermissionNames = ExpandGrantedPermissionNames(
            input.GrantedPermissionNames,
            permissionDefinitions
        );

        await ReplaceRolePermissionsAsync(
            normalizedRoleName,
            grantedPermissionNames,
            permissionDefinitions.Keys.ToHashSet(StringComparer.Ordinal)
        );

        return VerbClassApplicationMappers.ToRoleCommandResultDto(
            normalizedRoleName,
            grantedPermissionNames.Count,
            permissionDefinitions.Count
        );
    }

    private async Task EnsureManagedRolesExistAsync()
    {
        var tenantId = CurrentTenant.Id
            ?? throw new UserFriendlyException("Role management requires a tenant context.");
        var existingRoleNames = (await _roleRepository.GetListAsync())
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in VerbClassRoles.All.Where(roleName => !existingRoleNames.Contains(roleName)))
        {
            var role = new IdentityRole(_guidGenerator.Create(), roleName, tenantId);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new UserFriendlyException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }
        }
    }

    private async Task<IdentityRole[]> GetManagedRoleRecordsAsync()
    {
        var rolesByName = (await _roleRepository.GetListAsync())
            .Where(role => !string.IsNullOrWhiteSpace(role.Name) && VerbClassRoles.IsManagedRole(role.Name))
            .ToDictionary(role => role.Name!, StringComparer.OrdinalIgnoreCase);

        return VerbClassRoles.All
            .Where(rolesByName.ContainsKey)
            .Select(roleName => rolesByName[roleName])
            .ToArray();
    }

    private async Task<IdentityRole> GetManagedRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null || !VerbClassRoles.IsManagedRole(role.Name))
        {
            throw new UserFriendlyException("Role was not found.");
        }

        return role;
    }

    private async Task<Dictionary<string, int>> GetUserCountByRoleNameAsync(IEnumerable<IdentityRole> roles)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                continue;
            }

            counts[role.Name] = (await _userManager.GetUsersInRoleAsync(role.Name)).Count;
        }

        return counts;
    }

    private async Task<IdentityUser[]> GetUsersInRoleAsync(string roleName)
    {
        return (await _userManager.GetUsersInRoleAsync(roleName))
            .OrderBy(x => IdentityUserDisplayNameSupport.BuildDisplayName(x), StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.UserName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<Dictionary<string, PermissionDefinition>> GetManageablePermissionDefinitionsAsync()
    {
        var group = (await _permissionDefinitionManager.GetGroupsAsync())
            .FirstOrDefault(x => string.Equals(x.Name, VerbClassPermissions.GroupName, StringComparison.Ordinal));

        if (group is null)
        {
            return [];
        }

        var definitions = new Dictionary<string, PermissionDefinition>(StringComparer.Ordinal);

        foreach (var permission in group.Permissions)
        {
            definitions[permission.Name] = permission;
        }

        return definitions;
    }

    private async Task<Dictionary<string, int>> GetGrantedPermissionCountByRoleNameAsync(ISet<string> manageablePermissionNames)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in VerbClassRoles.All)
        {
            counts[roleName] = (await GetGrantedPermissionNamesAsync(roleName, manageablePermissionNames)).Count;
        }

        return counts;
    }

    private async Task<HashSet<string>> GetGrantedPermissionNamesAsync(string roleName, ISet<string> manageablePermissionNames)
    {
        return (await _permissionGrantRepository.GetListAsync(RolePermissionValueProvider.ProviderName, roleName))
            .Select(x => x.Name)
            .Where(manageablePermissionNames.Contains)
            .ToHashSet(StringComparer.Ordinal);
    }

    private RolePermissionItemDto[] BuildPermissionItems(
        IReadOnlyDictionary<string, PermissionDefinition> permissionDefinitions,
        ISet<string> grantedPermissionNames)
    {
        return permissionDefinitions.Values
            .Select(permission => BuildPermissionItem(permission, grantedPermissionNames))
            .ToArray();
    }

    private RolePermissionItemDto BuildPermissionItem(
        PermissionDefinition permission,
        ISet<string> grantedPermissionNames)
    {
        return VerbClassApplicationMappers.ToRolePermissionItemDto(
            permission,
            GetPermissionDisplayName(permission),
            grantedPermissionNames.Contains(permission.Name)
        );
    }

    private RolePermissionEntryDto BuildPermissionEntry(PermissionDefinition permission)
    {
        return VerbClassApplicationMappers.ToRolePermissionEntryDto(
            permission,
            GetPermissionDisplayName(permission)
        );
    }

    private static List<ValidationResult> ValidatePermissionNames(
        IEnumerable<string> grantedPermissionNames,
        IReadOnlyDictionary<string, PermissionDefinition> permissionDefinitions)
    {
        var requestedPermissions = grantedPermissionNames.ToHashSet(StringComparer.Ordinal);

        if (requestedPermissions.All(permissionDefinitions.ContainsKey))
        {
            return [];
        }

        return
        [
            new ValidationResult(
                "One or more permissions are not available.",
                [nameof(UpdateRolePermissionsInput.GrantedPermissionNames)]
            )
        ];
    }

    private static HashSet<string> ExpandGrantedPermissionNames(
        IEnumerable<string> grantedPermissionNames,
        IReadOnlyDictionary<string, PermissionDefinition> permissionDefinitions)
    {
        var expandedPermissionNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var permissionName in grantedPermissionNames)
        {
            if (!permissionDefinitions.ContainsKey(permissionName))
            {
                continue;
            }

            expandedPermissionNames.Add(permissionName);
        }

        return expandedPermissionNames;
    }

    private async Task ReplaceRolePermissionsAsync(
        string roleName,
        ISet<string> grantedPermissionNames,
        ISet<string> manageablePermissionNames)
    {
        var existingGrants = (await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            ))
            .Where(x => manageablePermissionNames.Contains(x.Name))
            .ToArray();
        var existingPermissionNames = existingGrants
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);
        var permissionsToRemove = existingGrants
            .Where(x => !grantedPermissionNames.Contains(x.Name))
            .ToArray();

        foreach (var grant in permissionsToRemove)
        {
            await _permissionGrantRepository.DeleteAsync(grant, true);
        }

        var permissionsToAdd = grantedPermissionNames
            .Where(x => !existingPermissionNames.Contains(x))
            .ToArray();

        if (permissionsToAdd.Length == 0)
        {
            return;
        }

        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            roleName,
            permissionsToAdd,
            CurrentTenant.Id
        );
    }

    private static void NormalizeInput(UpdateRolePermissionsInput input)
    {
        input.GrantedPermissionNames = input.GrantedPermissionNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeManagedRoleName(string? roleName)
    {
        var normalizedRoleName = roleName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedRoleName) || !VerbClassRoles.IsManagedRole(normalizedRoleName))
        {
            throw new UserFriendlyException("Role was not found.");
        }

        return VerbClassRoles.All.First(x => string.Equals(x, normalizedRoleName, StringComparison.OrdinalIgnoreCase));
    }

    private string GetPermissionDisplayName(PermissionDefinition permission)
    {
        var localizationKey = GetPermissionLocalizationKey(permission.Name);
        if (!string.IsNullOrWhiteSpace(localizationKey))
        {
            var localized = L[localizationKey];
            if (!localized.ResourceNotFound && !string.IsNullOrWhiteSpace(localized.Value))
            {
                return localized.Value;
            }
        }

        var label = permission.DisplayName.ToString();
        if (!string.IsNullOrWhiteSpace(label) &&
            !string.Equals(label, typeof(Volo.Abp.Localization.LocalizableString).FullName, StringComparison.Ordinal) &&
            !label.StartsWith("Permission:", StringComparison.Ordinal))
        {
            return label;
        }

        var segment = permission.Name.Split('.').Last();
        return PermissionLabelWordBoundaryRegex.Replace(segment, "$1 $2");
    }

    private static string? GetPermissionLocalizationKey(string permissionName)
    {
        var prefix = $"{VerbClassPermissions.GroupName}.";
        if (!permissionName.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return $"Permission:{permissionName[prefix.Length..]}";
    }
}
