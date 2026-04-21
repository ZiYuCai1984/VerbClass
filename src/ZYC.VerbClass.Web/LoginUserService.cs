using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace ZYC.VerbClass.Web;

internal class LoginUserService : ILoginUserService, ITransientDependency
{
    private readonly IdentityUserManager _userManager;

    public LoginUserService(IdentityUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> ResolveLoginNameAsync(string? userNameOrEmail)
    {
        var value = userNameOrEmail?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!value.Contains('@'))
        {
            return value;
        }

        var user = await _userManager.FindByEmailAsync(value);
        return user?.UserName;
    }
}