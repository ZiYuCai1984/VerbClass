using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class CourseOfferingParticipantRepository
    : EfCoreRepository<AcademicDbContext, CourseOfferingParticipant, Guid>,
        ICourseOfferingParticipantRepository
{
    public CourseOfferingParticipantRepository(IDbContextProvider<AcademicDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<CourseOfferingParticipant[]> GetListByOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.CourseOfferingId == courseOfferingId)
            .OrderBy(x => x.Role)
            .ThenBy(x => x.UserId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<CourseOfferingParticipant[]> GetListByOfferingsAsync(
        IReadOnlyCollection<Guid> courseOfferingIds,
        CancellationToken cancellationToken = default)
    {
        if (courseOfferingIds.Count == 0)
        {
            return [];
        }

        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => courseOfferingIds.Contains(x.CourseOfferingId))
            .OrderBy(x => x.CourseOfferingId)
            .ThenBy(x => x.Role)
            .ThenBy(x => x.UserId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<bool> IsUserAssignedAsync(
        Guid courseOfferingId,
        Guid userId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.CourseOfferingId == courseOfferingId &&
                 x.UserId == userId &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public async Task<bool> HasCourseOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.CourseOfferingId == courseOfferingId,
            cancellationToken
        );
    }
}
