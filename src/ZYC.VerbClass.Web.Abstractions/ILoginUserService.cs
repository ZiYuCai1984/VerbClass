namespace ZYC.VerbClass.Web.Abstractions;

public interface ILoginUserService
{
    Task<string?> ResolveLoginNameAsync(string? userNameOrEmail);
}

