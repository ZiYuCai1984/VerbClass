using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseDefinitionRepository
    : EfCoreRepository<AcademicDbContext, CourseDefinition, Guid>,
        ICourseDefinitionRepository
{
    public CourseDefinitionRepository(IDbContextProvider<AcademicDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<CourseDefinition?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.FirstOrDefaultAsync(
            x => x.Code == code,
            cancellationToken
        );
    }

    public async Task<bool> IsCodeExistsAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.Code == code &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }
}
