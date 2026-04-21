using Volo.Abp.DependencyInjection;

namespace ZYC.VerbClass.Domain.Data;

public class NullVerbClassDbSchemaMigrator : IVerbClassDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}