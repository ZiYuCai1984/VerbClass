using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class AcademicDbContextFactory : IDesignTimeDbContextFactory<AcademicDbContext>
{
    public AcademicDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<AcademicDbContext>()
            .UseSqlite(configuration.GetConnectionString("Default"));

        return new AcademicDbContext(builder.Options);
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
