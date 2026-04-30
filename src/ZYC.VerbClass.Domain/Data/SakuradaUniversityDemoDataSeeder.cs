using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace ZYC.VerbClass.Domain.Data;

public class SakuradaUniversityDemoDataSeeder : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<ISakuradaUniversityTenantDemoDataSeeder> _tenantDemoDataSeeders;
    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public SakuradaUniversityDemoDataSeeder(
        ICurrentTenant currentTenant,
        IDataSeeder dataSeeder,
        IEnumerable<ISakuradaUniversityTenantDemoDataSeeder> tenantDemoDataSeeders,
        TenantManager tenantManager,
        ITenantRepository tenantRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _currentTenant = currentTenant;
        _dataSeeder = dataSeeder;
        _tenantDemoDataSeeders = tenantDemoDataSeeders;
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
        _unitOfWorkManager = unitOfWorkManager;

        Logger = NullLogger<SakuradaUniversityDemoDataSeeder>.Instance;
    }

    public ILogger<SakuradaUniversityDemoDataSeeder> Logger { get; set; }

    public async Task SeedAsync()
    {
        Guid tenantId;
        using (var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
        {
            tenantId = await EnsureDemoTenantAsync();
            await unitOfWork.CompleteAsync();
        }

        var tenantDemoDataSeeders = _tenantDemoDataSeeders.OrderBy(x => x.Order).ToArray();
        if (tenantDemoDataSeeders.Length == 0)
        {
            throw new InvalidOperationException("No Sakura University tenant demo data seeders are registered.");
        }

        foreach (var tenantDemoDataSeeder in tenantDemoDataSeeders)
        {
            using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            await tenantDemoDataSeeder.SeedAsync(tenantId);
            await unitOfWork.CompleteAsync();
        }
    }

    private async Task<Guid> EnsureDemoTenantAsync()
    {
        using (_currentTenant.Change(null))
        {
            var tenant = await _tenantRepository.FindByNameAsync(SakuradaUniversitySeedData.DemoTenantName);
            if (tenant == null)
            {
                Logger.LogInformation(
                    "Creating Sakurada University demo tenant '{TenantName}'.",
                    SakuradaUniversitySeedData.DemoTenantName
                );

                tenant = await _tenantManager.CreateAsync(SakuradaUniversitySeedData.DemoTenantName);
                await _tenantRepository.InsertAsync(tenant, true);
            }

            using (_currentTenant.Change(tenant.Id))
            {
                await _dataSeeder.SeedAsync(
                    new DataSeedContext(tenant.Id)
                        .WithProperty(
                            IdentityDataSeedContributor.AdminEmailPropertyName,
                            SakuradaUniversitySeedData.DemoTenantAdminEmail
                        )
                        .WithProperty(
                            IdentityDataSeedContributor.AdminPasswordPropertyName,
                            SakuradaUniversitySeedData.DemoAdminPassword
                        )
                );
            }

            return tenant.Id;
        }
    }
}
