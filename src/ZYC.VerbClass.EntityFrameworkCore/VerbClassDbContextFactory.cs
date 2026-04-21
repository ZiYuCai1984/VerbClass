using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ZYC.VerbClass.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class VerbClassDbContextFactory : IDesignTimeDbContextFactory<VerbClassDbContext>
{
    public VerbClassDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        VerbClassEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<VerbClassDbContext>()
            .UseSqlite(configuration.GetConnectionString("Default"));

        return new VerbClassDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ZYC.VerbClass.DbMigrator/"))
            .AddJsonFile("appsettings.json", false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}