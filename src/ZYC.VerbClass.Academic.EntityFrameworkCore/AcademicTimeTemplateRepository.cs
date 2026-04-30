using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public class AcademicTimeTemplateRepository
    : EfCoreRepository<AcademicDbContext, AcademicTimeTemplate, Guid>,
        IAcademicTimeTemplateRepository
{
    public AcademicTimeTemplateRepository(IDbContextProvider<AcademicDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<AcademicTimeTemplate?> FindByCodeAsync(
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
