using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace ZYC.VerbClass.Domain.Data;

[ExposeServices(typeof(SakuradaUniversityIdentityDemoDataSeeder), typeof(ISakuradaUniversityTenantDemoDataSeeder))]
public class SakuradaUniversityIdentityDemoDataSeeder : ISakuradaUniversityTenantDemoDataSeeder, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IdentityRoleManager _roleManager;
    private readonly ITenantRepository _tenantRepository;
    private readonly IdentityUserManager _userManager;

    public SakuradaUniversityIdentityDemoDataSeeder(
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IdentityRoleManager roleManager,
        ITenantRepository tenantRepository,
        IdentityUserManager userManager)
    {
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _roleManager = roleManager;
        _tenantRepository = tenantRepository;
        _userManager = userManager;
    }

    public int Order => 10;

    public async Task SeedAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.FindAsync(tenantId)
                     ?? throw new AbpException($"Tenant '{tenantId}' was not found for Sakura demo identity seeding.");

        if (!string.Equals(tenant.Name, SakuradaUniversitySeedData.DemoTenantName, StringComparison.Ordinal))
        {
            throw new AbpException(
                $"Tenant '{tenant.Name}' is not the Sakura University demo tenant."
            );
        }

        await EnsureDemoAccountsAsync(tenantId);
    }

    public async Task<Dictionary<string, IdentityUser>> EnsureDemoAccountsAsync(Guid tenantId)
    {
        using (_currentTenant.Change(tenantId))
        {
            var usersByUserName = new Dictionary<string, IdentityUser>(StringComparer.OrdinalIgnoreCase);

            foreach (var roleName in SakuradaUniversitySeedData.UserSeedItems
                         .SelectMany(x => x.RoleNames)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                await EnsureRoleAsync(roleName, tenantId);
            }

            foreach (var seedItem in SakuradaUniversitySeedData.UserSeedItems)
            {
                var user = await EnsureUserAsync(seedItem, tenantId);
                await EnsureUserRolesAsync(user, seedItem.RoleNames);
                usersByUserName[seedItem.UserName] = user;
            }

            return usersByUserName;
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
        await CheckAsync(await _roleManager.CreateAsync(role));
    }

    private async Task<IdentityUser> EnsureUserAsync(UniversityUserSeedItem seedItem, Guid tenantId)
    {
        var user = await _userManager.FindByNameAsync(seedItem.UserName);
        if (user is null)
        {
            user = new IdentityUser(
                _guidGenerator.Create(),
                seedItem.UserName,
                seedItem.Email,
                tenantId
            )
            {
                Name = seedItem.NameKanji,
                Surname = seedItem.SurnameKanji
            };

            await CheckAsync(await _userManager.CreateAsync(user, seedItem.Password, false));
            return user;
        }

        var changed = false;

        if (!string.Equals(user.Name, seedItem.NameKanji, StringComparison.Ordinal))
        {
            user.Name = seedItem.NameKanji;
            changed = true;
        }

        if (!string.Equals(user.Surname, seedItem.SurnameKanji, StringComparison.Ordinal))
        {
            user.Surname = seedItem.SurnameKanji;
            changed = true;
        }

        if (changed)
        {
            await CheckAsync(await _userManager.UpdateAsync(user));
        }

        return user;
    }

    private async Task EnsureUserRolesAsync(IdentityUser user, IReadOnlyList<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                continue;
            }

            await CheckAsync(await _userManager.AddToRoleAsync(user, roleName));
        }
    }

    private static Task CheckAsync(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return Task.CompletedTask;
        }

        var errorMessage = string.Join("; ", result.Errors.Select(x => x.Description));
        throw new AbpException(errorMessage);
    }
}
