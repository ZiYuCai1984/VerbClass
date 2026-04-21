namespace ZYC.VerbClass.Application.Contracts.Roles;

public interface IUserRolesAppService
{
    Task<string[]> GetRolesAsync(Guid userId);
}