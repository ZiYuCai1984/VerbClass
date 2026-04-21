using Volo.Abp.Domain.Repositories;

namespace ZYC.VerbClass.Domain.Departments;

public interface IDepartmentRepository : IRepository<Department, Guid>
{
    Task<Department?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<List<Department>> GetChildrenAsync(
        Guid? parentDepartmentId,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsCodeExistsAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsDescendantOfAsync(
        Guid departmentId,
        Guid possibleAncestorId,
        CancellationToken cancellationToken = default
    );
}