using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseOfferingRepository
    : EfCoreRepository<AcademicDbContext, CourseOffering, Guid>,
        ICourseOfferingRepository
{
    public CourseOfferingRepository(IDbContextProvider<AcademicDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<CourseOffering[]> GetListByTermAsync(
        Guid academicTermId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.AcademicTermId == academicTermId)
            .OrderBy(x => x.OfferingCode)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> IsOfferingCodeExistsAsync(
        Guid academicTermId,
        string offeringCode,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.AcademicTermId == academicTermId &&
                 x.OfferingCode == offeringCode &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public async Task<bool> HasCourseDefinitionAsync(
        Guid courseDefinitionId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.CourseDefinitionId == courseDefinitionId,
            cancellationToken
        );
    }

    public async Task<bool> HasAcademicTermAsync(
        Guid academicTermId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.AcademicTermId == academicTermId,
            cancellationToken
        );
    }
}
