using System.Diagnostics;
using Microsoft.Extensions.Configuration;
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
    private const string SeedSakuradaUniversityDemoDataConfigurationKey =
        "VerbClass:DataSeed:SeedSakuradaUniversityDemoData";

    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;

    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<IVerbClassDbSchemaMigrator> _dbSchemaMigrators;
    private readonly SakuradaUniversityDemoDataSeeder _sakuradaUniversityDemoDataSeeder;
    private readonly ITenantRepository _tenantRepository;

    public VerbClassDbMigrationService(
        IConfiguration configuration,
        IDataSeeder dataSeeder,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IEnumerable<IVerbClassDbSchemaMigrator> dbSchemaMigrators,
        SakuradaUniversityDemoDataSeeder sakuradaUniversityDemoDataSeeder)
    {
        _configuration = configuration;
        _dataSeeder = dataSeeder;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
        _dbSchemaMigrators = dbSchemaMigrators;
        _sakuradaUniversityDemoDataSeeder = sakuradaUniversityDemoDataSeeder;

        Logger = NullLogger<VerbClassDbMigrationService>.Instance;
    }

    public ILogger<VerbClassDbMigrationService> Logger { get; }


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

        var seedSakuradaUniversityDemoData = IsSakuradaUniversityDemoDataSeedEnabled();
        if (seedSakuradaUniversityDemoData)
        {
            await _sakuradaUniversityDemoDataSeeder.SeedAsync();
        }

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
    }

    private bool IsSakuradaUniversityDemoDataSeedEnabled()
    {
        var configuredValue = _configuration[SeedSakuradaUniversityDemoDataConfigurationKey];
        var enabled = false;
        if (!string.IsNullOrWhiteSpace(configuredValue) && !bool.TryParse(configuredValue, out enabled))
        {
            throw new InvalidOperationException(
                $"Configuration '{SeedSakuradaUniversityDemoDataConfigurationKey}' must be a boolean value."
            );
        }

        Logger.LogInformation(
            "Sakurada University demo data seeding is {State}.",
            enabled ? "enabled" : "disabled"
        );

        return enabled;
    }

    private bool AddInitialMigrationIfNotExist()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();
        var migrationsFolder = Path.Combine(dbMigrationsProjectFolder, "Migrations");
        if (Directory.Exists(migrationsFolder))
        {
            return false;
        }

        Logger.LogInformation(
            "No migrations folder found under '{DbMigrationsProjectFolder}'. Creating initial migration.",
            dbMigrationsProjectFolder
        );

        AddInitialMigration(dbMigrationsProjectFolder);
        return true;
    }

    private void AddInitialMigration(string dbMigrationsProjectFolder)
    {
        Logger.LogInformation("Creating initial migration...");

        var procStartInfo = new ProcessStartInfo
        {
            FileName = "abp",
            UseShellExecute = false
        };

        procStartInfo.ArgumentList.Add("create-migration-and-run-migrator");
        procStartInfo.ArgumentList.Add(dbMigrationsProjectFolder);

        try
        {
            _ = Process.Start(procStartInfo)
                                ?? throw new InvalidOperationException("ABP CLI process could not be started.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to run ABP CLI for '{dbMigrationsProjectFolder}'.",
                ex
            );
        }
    }

    private string GetEntityFrameworkCoreProjectFolderPath()
    {
        var slnDirectoryPath = GetSolutionDirectoryPath();
        if (slnDirectoryPath is null)
        {
            throw new InvalidOperationException(
                $"Solution folder was not found from '{Directory.GetCurrentDirectory()}'."
            );
        }

        var dbMigrationsProjectFolder = Path.Combine(slnDirectoryPath, "ZYC.VerbClass.EntityFrameworkCore");
        if (!Directory.Exists(dbMigrationsProjectFolder))
        {
            throw new InvalidOperationException(
                $"EntityFrameworkCore project folder was not found at '{dbMigrationsProjectFolder}'."
            );
        }

        return dbMigrationsProjectFolder;
    }

    private string? GetSolutionDirectoryPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory != null)
        {
            if (ContainsSolutionFile(currentDirectory.FullName))
            {
                return currentDirectory.FullName;
            }

            var srcDirectoryPath = Path.Combine(currentDirectory.FullName, "src");
            if (Directory.Exists(srcDirectoryPath) && ContainsSolutionFile(srcDirectoryPath))
            {
                return srcDirectoryPath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static bool ContainsSolutionFile(string directoryPath)
    {
        return Directory.GetFiles(directoryPath).Any(f => f.EndsWith(".sln") || f.EndsWith(".slnx"));
    }

}
