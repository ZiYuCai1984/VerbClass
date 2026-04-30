using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;

namespace ZYC.VerbClass.Academic.Domain.Data;

internal class AcademicTermDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly AcademicTermManager _academicTermManager;
    private readonly IAcademicTermRepository _academicTermRepository;

    public AcademicTermDataSeedContributor(
        AcademicTermManager academicTermManager,
        IAcademicTermRepository academicTermRepository)
    {
        _academicTermManager = academicTermManager;
        _academicTermRepository = academicTermRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId == null)
        {
            return;
        }

        await CreateAndInsertAsync(
            2026,
            "2026-first-semester",
            "前期",
            new DateTime(2026, 4, 1),
            new DateTime(2026, 9, 30),
            CreateTermPeriods());

        await CreateAndInsertAsync(
            2026,
            "2026-second-semester",
            "後期",
            new DateTime(2026, 10, 1),
            new DateTime(2027, 3, 31),
            CreateTermPeriods());
    }

    private async Task CreateAndInsertAsync(
        int academicYear,
        string code,
        string name,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<TermPeriodDefinition> periods)
    {
        var term = await _academicTermRepository.FindByYearAndCodeAsync(academicYear, code);
        if (term != null)
        {
            return;
        }

        term = await _academicTermManager.CreateAsync(
            academicYear,
            code,
            name,
            startDate,
            endDate
            , periods);

        await _academicTermRepository.InsertAsync(term, true);
    }

    private static TermPeriodDefinition[] CreateTermPeriods()
    {
        return AcademicTimeTemplateDataSeedContributor.SeedItems[0].Periods
            .Select(period => TermPeriodDefinition.Create(
                period.PeriodNo,
                period.Label,
                period.StartTime,
                period.EndTime
            ))
            .ToArray();
    }
}
