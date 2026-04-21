using Microsoft.AspNetCore.Http;

namespace ZYC.VerbClass.Web;

public partial class VerbClassWebModule
{
    private static readonly HashSet<string> BlockedPagePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Identity/Users/Index",
        "/Identity/Users/CreateModal",
        "/Identity/Users/EditModal",
        "/Identity/Roles/Index",
        "/Identity/Roles/CreateModal",
        "/Identity/Roles/EditModal",
        "/Account/Manage",
        "/TenantManagement/Tenants/Index",
        "/TenantManagement/Tenants/CreateModal",
        "/TenantManagement/Tenants/EditModal"
    };

    private static bool IsBlockedPagePath(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Length > 1)
        {
            value = value.TrimEnd('/');
        }

        return BlockedPagePaths.Contains(value);
    }
}
