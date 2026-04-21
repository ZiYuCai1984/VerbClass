using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Application.Contracts.Users;

public interface IUserManagementAppService : IApplicationService
{
    Task<UserPermissionsDto> GetPermissionsAsync();

    Task<UserListItemDto[]> GetListAsync();

    Task<UserDetailDto> GetAsync(Guid userId);

    Task<UserEditorDto> GetEditorAsync(Guid userId);

    Task<string[]> GetAssignableRoleNamesAsync();

    Task<UserCommandResultDto> CreateAsync(CreateUserInput input);

    Task<UserCommandResultDto> UpdateAsync(Guid userId, UpdateUserInput input);

    Task<UserCommandResultDto> DeleteAsync(Guid userId);
}
