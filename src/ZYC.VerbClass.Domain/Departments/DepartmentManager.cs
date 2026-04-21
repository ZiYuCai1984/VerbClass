using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Departments;

public class DepartmentManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentManager(
        ICurrentTenant currentTenant,
        IDepartmentRepository departmentRepository)
    {
        _currentTenant = currentTenant;
        _departmentRepository = departmentRepository;
    }

    public virtual async Task<Department> CreateAsync(
        string code,
        string name,
        Guid? parentDepartmentId = null,
        string? shortName = null,
        int sort = 0,
        bool canAssignUsers = true,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null,
        CancellationToken cancellationToken = default)
    {
        await ValidateCodeAsync(code, null, cancellationToken);
        await ValidateParentAsync(parentDepartmentId, cancellationToken);

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("Department must be created within a tenant context.");

        var department = new Department(
            GuidGenerator.Create(),
            tenantId,
            code,
            name,
            parentDepartmentId,
            shortName,
            sort,
            canAssignUsers,
            effectiveFrom,
            effectiveTo
        );

        var path = await BuildPathAsync(parentDepartmentId, department.Id, cancellationToken);
        department.SetPath(path);

        return department;
    }

    public virtual async Task ChangeCodeAsync(
        Department department,
        string newCode,
        CancellationToken cancellationToken = default)
    {
        await ValidateCodeAsync(newCode, department.Id, cancellationToken);
        department.SetCode(newCode);
    }

    public virtual async Task ChangeParentAsync(
        Department department,
        Guid? newParentDepartmentId,
        CancellationToken cancellationToken = default)
    {
        if (newParentDepartmentId == department.Id)
        {
            throw new BusinessException(DepartmentErrorCodes.ParentCannotBeSelf);
        }

        await ValidateParentAsync(newParentDepartmentId, cancellationToken);

        if (newParentDepartmentId.HasValue)
        {
            var isCircular = await _departmentRepository.IsDescendantOfAsync(
                newParentDepartmentId.Value,
                department.Id,
                cancellationToken
            );

            if (isCircular)
            {
                throw new BusinessException(DepartmentErrorCodes.CircularHierarchy);
            }
        }

        department.ChangeParent(newParentDepartmentId);

        var newPath = await BuildPathAsync(newParentDepartmentId, department.Id, cancellationToken);
        department.SetPath(newPath);

        //!WARNING This only updates the current department path.
        // Descendant paths must be refreshed by the application layer after the move.
    }

    protected virtual async Task ValidateCodeAsync(
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _departmentRepository.IsCodeExistsAsync(
            code,
            excludeId,
            cancellationToken
        );

        if (exists)
        {
            throw new BusinessException(DepartmentErrorCodes.CodeAlreadyExists)
                .WithData("Code", code);
        }
    }

    protected virtual async Task ValidateParentAsync(
        Guid? parentDepartmentId,
        CancellationToken cancellationToken)
    {
        if (!parentDepartmentId.HasValue)
        {
            return;
        }

        var parent = await _departmentRepository.FindAsync(
            parentDepartmentId.Value,
            cancellationToken: cancellationToken
        );

        if (parent is null)
        {
            throw new BusinessException(DepartmentErrorCodes.ParentNotFound)
                .WithData("ParentDepartmentId", parentDepartmentId.Value);
        }

        if (!parent.IsActive)
        {
            throw new BusinessException(DepartmentErrorCodes.ParentIsInactive)
                .WithData("ParentDepartmentId", parentDepartmentId.Value);
        }
    }

    protected virtual async Task<string> BuildPathAsync(
        Guid? parentDepartmentId,
        Guid currentDepartmentId,
        CancellationToken cancellationToken)
    {
        if (!parentDepartmentId.HasValue)
        {
            return $"/{currentDepartmentId}/";
        }

        var parent = await _departmentRepository.GetAsync(
            parentDepartmentId.Value,
            cancellationToken: cancellationToken
        );

        return $"{parent.Path}{currentDepartmentId}/";
    }
}
