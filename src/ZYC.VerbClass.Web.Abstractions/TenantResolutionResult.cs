namespace ZYC.VerbClass.Web.Abstractions;

public record TenantResolutionResult(bool IsValid, Guid? Id, string? Name)
{
    public static TenantResolutionResult Host()
    {
        return new TenantResolutionResult(true, null, null);
    }

    public static TenantResolutionResult ForTenant(Guid id, string name)
    {
        return new TenantResolutionResult(true, id, name);
    }

    public static TenantResolutionResult Invalid()
    {
        return new TenantResolutionResult(false, null, null);
    }
}