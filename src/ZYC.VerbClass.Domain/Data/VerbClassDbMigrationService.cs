using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace ZYC.VerbClass.Domain.Data;

public class VerbClassDbMigrationService : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;

    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<IVerbClassDbSchemaMigrator> _dbSchemaMigrators;
    private readonly VerbClassPermissionDataSeedContributor _permissionDataSeedContributor;

    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;

    public VerbClassDbMigrationService(
        TenantManager tenantManager,
        IDataSeeder dataSeeder,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IEnumerable<IVerbClassDbSchemaMigrator> dbSchemaMigrators,
        VerbClassPermissionDataSeedContributor permissionDataSeedContributor)
    {
        _tenantManager = tenantManager;
        _dataSeeder = dataSeeder;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
        _dbSchemaMigrators = dbSchemaMigrators;
        _permissionDataSeedContributor = permissionDataSeedContributor;

        Logger = NullLogger<VerbClassDbMigrationService>.Instance;
    }

    public ILogger<VerbClassDbMigrationService> Logger { get; }


    private async Task EnsureInitialTenantAsync()
    {
        //Create a tenant in the host context
        using (_currentTenant.Change(null))
        {
            var tenant = await _tenantRepository.FindByNameAsync(SakuradaUniversitySeedData.InitialTenantName);
            if (tenant == null)
            {
                tenant = await _tenantManager.CreateAsync(SakuradaUniversitySeedData.InitialTenantName);
                await _tenantRepository.InsertAsync(tenant, true);

                await _dataSeeder.SeedAsync(
                    new DataSeedContext(tenant.Id)
                        .WithProperty(
                            IdentityDataSeedContributor.AdminEmailPropertyName,
                            SakuradaUniversitySeedData.InitialTenantAdminEmail
                        )
                        .WithProperty(
                            IdentityDataSeedContributor.AdminPasswordPropertyName,
                            SakuradaUniversitySeedData.InitialAdminPassword
                        )
                );
            }
        }
    }

    public async Task MigrateAsync()
    {
        var initialMigrationAdded = AddInitialMigrationIfNotExist();

        if (initialMigrationAdded)
        {
            return;
        }

        Logger.LogInformation("Started database migrations...");

        await MigrateDatabaseSchemaAsync();
        await SeedDataAsync();

        await EnsureInitialTenantAsync();

        Logger.LogInformation("Successfully completed host database migrations.");

        var tenants = await _tenantRepository.GetListAsync(includeDetails: true);

        var migratedDatabaseSchemas = new HashSet<string>();
        foreach (var tenant in tenants)
        {
            using (_currentTenant.Change(tenant.Id))
            {
                if (tenant.ConnectionStrings.Any())
                {
                    var tenantConnectionStrings = tenant.ConnectionStrings
                        .Select(x => x.Value)
                        .ToList();

                    if (!migratedDatabaseSchemas.IsSupersetOf(tenantConnectionStrings))
                    {
                        await MigrateDatabaseSchemaAsync(tenant);

                        migratedDatabaseSchemas.AddIfNotContains(tenantConnectionStrings);
                    }
                }

                await SeedDataAsync(tenant);
            }

            Logger.LogInformation($"Successfully completed {tenant.Name} tenant database migrations.");
        }

        Logger.LogInformation("Successfully completed all database migrations.");


        Logger.LogInformation("You can safely end this process...");
    }

    private async Task MigrateDatabaseSchemaAsync(Tenant? tenant = null)
    {
        Logger.LogInformation(
            $"Migrating schema for {(tenant == null ? "host" : tenant.Name + " tenant")} database...");

        foreach (var migrator in _dbSchemaMigrators)
        {
            await migrator.MigrateAsync();
        }
    }

    private async Task SeedDataAsync(Tenant? tenant = null)
    {
        Logger.LogInformation($"Executing {(tenant == null ? "host" : tenant.Name + " tenant")} database seed...");

        await _dataSeeder.SeedAsync(new DataSeedContext(tenant?.Id)
            .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName,
                VerbClassConsts.AdminEmailDefaultValue)
            .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName,
                VerbClassConsts.AdminPasswordDefaultValue)
        );

        if (tenant != null)
        {
            await _permissionDataSeedContributor.SyncTenantPermissionsAsync(tenant.Id);
        }
    }

    private bool AddInitialMigrationIfNotExist()
    {
        try
        {
            if (!DbMigrationsProjectExists())
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            if (!MigrationsFolderExists())
            {
                AddInitialMigration();
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Logger.LogWarning("Couldn't determinate if any migrations exist : " + e.Message);
            return false;
        }
    }

    private bool DbMigrationsProjectExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

        return dbMigrationsProjectFolder != null;
    }

    private bool MigrationsFolderExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

        return dbMigrationsProjectFolder != null &&
               Directory.Exists(Path.Combine(dbMigrationsProjectFolder, "Migrations"));
    }

    private void AddInitialMigration()
    {
        Logger.LogInformation("Creating initial migration...");

        string argumentPrefix;
        string fileName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            argumentPrefix = "-c";
            fileName = "/bin/bash";
        }
        else
        {
            argumentPrefix = "/C";
            fileName = "cmd.exe";
        }

        var procStartInfo = new ProcessStartInfo(fileName,
            $"{argumentPrefix} \"abp create-migration-and-run-migrator \"{GetEntityFrameworkCoreProjectFolderPath()}\"\""
        );

        try
        {
            Process.Start(procStartInfo);
        }
        catch (Exception)
        {
            throw new Exception("Couldn't run ABP CLI...");
        }
    }

    private string? GetEntityFrameworkCoreProjectFolderPath()
    {
        var slnDirectoryPath = GetSolutionDirectoryPath();

        if (slnDirectoryPath == null)
        {
            throw new Exception("Solution folder not found!");
        }

        //TODO-zyc GetEntityFrameworkCoreProjectFolderPath

        return Directory.GetDirectories(slnDirectoryPath)
            .FirstOrDefault(d => d.EndsWith("ZYC.VerbClass.EntityFrameworkCore"));
    }

    private string? GetSolutionDirectoryPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory != null && Directory.GetParent(currentDirectory.FullName) != null)
        {
            currentDirectory = Directory.GetParent(currentDirectory.FullName);

            if (currentDirectory != null && Directory.GetFiles(currentDirectory.FullName)
                    .FirstOrDefault(f => f.EndsWith(".sln") || f.EndsWith(".slnx")) != null)
            {
                return currentDirectory.FullName;
            }
        }

        return null;
    }
}
