using Volo.Abp.Domain.Repositories;

namespace ZYC.VerbClass.Academic.Domain.AcademicTerms;

public interface IAcademicTermRepository : IRepository<AcademicTerm, Guid>
{
    Task<AcademicTerm?> FindByYearAndCodeAsync(
        int academicYear,
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> IsCodeExistsAsync(
        int academicYear,
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
