using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ZYC.VerbClass.DbMigrator;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
            .MinimumLevel.Override("ZYC.VerbClass", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("ZYC.VerbClass", LogEventLevel.Information)
#endif
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }

    //public static IHostBuilder CreateHostBuilder(string[] args) =>
    //    Host.CreateDefaultBuilder(args)
    //        .AddAppSettingsSecretsJson()
    //        .ConfigureLogging((context, logging) => logging.ClearProviders())
    //        .ConfigureServices((hostContext, services) =>
    //        {
    //            services.AddHostedService<DbMigratorHostedService>();
    //        });


    public static IHostBuilder CreateHostBuilder(string[] args)
    {

        return Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .ConfigureAppConfiguration((context, config) =>
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "VerbClass.db");
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    //TODO-zyc Temp code(ConnectionStrings)
                    ["ConnectionStrings:Default"] = $"Data Source={dbPath};"
                });
            })
            .ConfigureLogging((context, logging) => logging.ClearProviders())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<DbMigratorHostedService>();
            });
    }
}