using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Domain.Data;

namespace ZYC.VerbClass.EntityFrameworkCore;

public class EntityFrameworkCoreVerbClassDbSchemaMigrator
    : IVerbClassDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreVerbClassDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the VerbClassDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<VerbClassDbContext>()
            .Database
            .MigrateAsync();
    }
}
