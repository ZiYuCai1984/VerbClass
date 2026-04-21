using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Application.Contracts.Roles;

public interface IRoleManagementAppService : IApplicationService
{
    Task<RolePermissionsDto> GetPermissionsAsync();

    Task<RoleListItemDto[]> GetListAsync();

    Task<RoleDetailDto> GetAsync(string roleName);

    Task<RolePermissionEditorDto> GetEditorAsync(string roleName);

    Task<RoleCommandResultDto> UpdatePermissionsAsync(string roleName, UpdateRolePermissionsInput input);
}
