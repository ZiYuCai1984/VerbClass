using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Domain.Departments;

namespace ZYC.VerbClass.EntityFrameworkCore;

public class DepartmentRepository
    : EfCoreRepository<VerbClassDbContext, Department, Guid>,
        IDepartmentRepository
{
    public DepartmentRepository(IDbContextProvider<VerbClassDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Department?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public async Task<List<Department>> GetChildrenAsync(
        Guid? parentDepartmentId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.ParentDepartmentId == parentDepartmentId)
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCodeExistsAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.Code == code && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public async Task<bool> IsDescendantOfAsync(
        Guid departmentId,
        Guid possibleAncestorId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        var department = await dbSet
            .Where(x => x.Id == departmentId)
            .Select(x => new { x.Path })
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
        {
            return false;
        }

        var marker = $"/{possibleAncestorId}/";
        return department.Path.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }
}