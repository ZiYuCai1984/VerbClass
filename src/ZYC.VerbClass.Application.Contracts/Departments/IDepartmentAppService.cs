using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Application.Contracts.Departments;

public interface IDepartmentAppService : IApplicationService
{
    Task<DepartmentPermissionsDto> GetPermissionsAsync();

    Task<DepartmentListItemDto[]> GetListAsync();

    Task<DepartmentDetailDto> GetAsync(Guid departmentId);

    Task<DepartmentEditorDto> GetEditorAsync(Guid departmentId);

    Task<DepartmentParentOptionDto[]> GetParentOptionsAsync(Guid? departmentId = null);

    Task<DepartmentCommandResultDto> CreateAsync(CreateDepartmentInput input);

    Task<DepartmentCommandResultDto> UpdateAsync(Guid departmentId, UpdateDepartmentInput input);

    Task<DepartmentCommandResultDto> DeleteAsync(Guid departmentId);

    Task<Guid[]> GetExistingIdsAsync(Guid[] departmentIds);

    Task<UserDepartmentOptionDto[]> GetUserDepartmentOptionsAsync(Guid[] selectedDepartmentIds);

    Task<UserDepartmentDisplayItemDto[]> GetUserDepartmentDisplayItemsAsync(Guid userId);

    Task<UserDepartmentSummaryDto[]> GetUserDepartmentSummariesAsync(Guid[] userIds);
}
