using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Data;

public class VerbClassPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly string[] InitialOperationsAdminPermissions;

    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly IdentityRoleManager _roleManager;

    static VerbClassPermissionDataSeedContributor()
    {
        InitialOperationsAdminPermissions = VerbClassPermissions.GetPermissions().Select(t => t.Name).ToArray();
    }

    public VerbClassPermissionDataSeedContributor(
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IPermissionGrantRepository permissionGrantRepository,
        IPermissionDataSeeder permissionDataSeeder,
        IdentityRoleManager roleManager)
    {
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _permissionGrantRepository = permissionGrantRepository;
        _permissionDataSeeder = permissionDataSeeder;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (!context.TenantId.HasValue)
        {
            return;
        }

        await SyncTenantPermissionsAsync(context.TenantId.Value);
    }

    public async Task SyncTenantPermissionsAsync(Guid tenantId)
    {
        using (_currentTenant.Change(tenantId))
        {
            await EnsureManagedRolesAsync(tenantId);
            await SeedRolePermissionsAsync(
                VerbClassRoles.OperationsAdmin,
                tenantId,
                InitialOperationsAdminPermissions
            );
        }
    }

    private async Task EnsureManagedRolesAsync(Guid tenantId)
    {
        foreach (var roleName in VerbClassRoles.All)
        {
            await EnsureRoleAsync(roleName, tenantId);
        }
    }

    private async Task EnsureRoleAsync(string roleName, Guid tenantId)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role != null)
        {
            return;
        }

        role = new IdentityRole(_guidGenerator.Create(), roleName, tenantId);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException(errorMessage);
        }
    }

    private async Task SeedRolePermissionsAsync(string roleName, Guid tenantId, string[] permissionNames)
    {
        var existingPermissionNames = (await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            ))
            .Select(x => x.Name)
            .ToHashSet(StringComparer.Ordinal);
        var missingPermissionNames = permissionNames
            .Where(x => !existingPermissionNames.Contains(x))
            .ToArray();

        if (missingPermissionNames.Length == 0)
        {
            return;
        }

        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            roleName,
            missingPermissionNames,
            tenantId
        );
    }

    private async Task RemoveRolePermissionsAsync(string roleName, string[] permissionNames)
    {
        var grantsToRemove = (await _permissionGrantRepository.GetListAsync(
                RolePermissionValueProvider.ProviderName,
                roleName
            ))
            .Where(x => permissionNames.Contains(x.Name, StringComparer.Ordinal))
            .ToArray();

        foreach (var grant in grantsToRemove)
        {
            await _permissionGrantRepository.DeleteAsync(grant, true);
        }
    }
}
