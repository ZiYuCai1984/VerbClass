using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.DependencyInjection;
using Volo.Abp.TenantManagement;

namespace ZYC.VerbClass.Web;

public class LoginTenantService : ILoginTenantService, ITransientDependency
{
    private readonly AbpAspNetCoreMultiTenancyOptions _multiTenancyOptions;
    private readonly ITenantRepository _tenantRepository;


    public LoginTenantService(
        ITenantRepository tenantRepository,
        IOptions<AbpAspNetCoreMultiTenancyOptions> multiTenancyOptions)
    {
        _tenantRepository = tenantRepository;
        _multiTenancyOptions = multiTenancyOptions.Value;
    }


    public async Task<TenantResolutionResult> ResolveTenantAsync(string? tenantName)
    {
        var value = tenantName?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return TenantResolutionResult.Host();
        }

        var tenant = await _tenantRepository.FindByNameAsync(value);
        return tenant is null
            ? TenantResolutionResult.Invalid()
            : TenantResolutionResult.ForTenant(tenant.Id, tenant.Name);
    }

    public void SetTenantCookie(object response, string? tenantName)
    {
        SetTenantCookie((HttpResponse)response, tenantName);
    }

    public string GetTenantDisplayName(string? tenantName)
    {
        return string.IsNullOrWhiteSpace(tenantName)
            ? "Host"
            : tenantName.Trim();
    }

    public void SetTenantCookie(HttpResponse response, string? tenantName)
    {
        var cookieName = _multiTenancyOptions.TenantKey;
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        if (string.IsNullOrWhiteSpace(tenantName))
        {
            response.Cookies.Delete(cookieName, cookieOptions);
            return;
        }

        response.Cookies.Append(cookieName, tenantName, cookieOptions);
    }
}