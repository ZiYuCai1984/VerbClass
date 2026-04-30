namespace ZYC.VerbClass.Domain.Data;

public interface ISakuradaUniversityTenantDemoDataSeeder
{
    int Order { get; }

    Task SeedAsync(Guid tenantId);
}
