using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace ZYC.VerbClass.Web.Abstractions;

public class UserTenantContext
{
    public UserTenantContext(ICurrentTenant? currentTenant, ICurrentUser? currentUser)
    {
        CurrentTenant = currentTenant;
        CurrentUser = currentUser;
    }

    public ICurrentUser? CurrentUser { get; }

    public ICurrentTenant? CurrentTenant { get; }

    public string GetTenantUserDisplayName()
    {
        var tenantName = CurrentTenant?.Name;
        var userName = CurrentUser?.UserName;

        var normalizedUserName = string.IsNullOrWhiteSpace(userName) ? "User" : userName.Trim();
        return string.IsNullOrWhiteSpace(tenantName)
            ? normalizedUserName
            : $"{tenantName.Trim()} / {normalizedUserName}";
    }
}