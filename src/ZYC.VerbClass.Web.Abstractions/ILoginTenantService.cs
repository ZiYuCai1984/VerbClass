namespace ZYC.VerbClass.Web.Abstractions;

public interface ILoginTenantService
{
    Task<TenantResolutionResult> ResolveTenantAsync(string? tenantName);
    
    void SetTenantCookie(object response, string? tenantName);

    string GetTenantDisplayName(string? tenantName);
}