using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class AcademicTermRepository
    : EfCoreRepository<AcademicDbContext, AcademicTerm, Guid>,
        IAcademicTermRepository
{
    public AcademicTermRepository(IDbContextProvider<AcademicDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<AcademicTerm?> FindByYearAndCodeAsync(
        int academicYear,
        string code,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.FirstOrDefaultAsync(
            x => x.AcademicYear == academicYear && x.Code == code,
            cancellationToken
        );
    }

    public async Task<bool> IsCodeExistsAsync(
        int academicYear,
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.AcademicYear == academicYear &&
                 x.Code == code &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }
}
