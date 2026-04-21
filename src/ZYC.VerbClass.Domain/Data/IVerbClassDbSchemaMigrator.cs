namespace ZYC.VerbClass.Domain.Data;

public interface IVerbClassDbSchemaMigrator
{
    Task MigrateAsync();
}
