using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.AcademicTerms;

[Audited]
public class AcademicTerm : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<TermPeriodDefinition> _periods = [];

    protected AcademicTerm()
    {
    }

    public AcademicTerm(
        Guid id,
        Guid tenantId,
        int academicYear,
        string code,
        string name,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<TermPeriodDefinition> periods) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        ChangeIdentity(academicYear, code);
        ChangeName(name);
        ChangeDateRange(startDate, endDate);
        ReplacePeriods(periods);
    }

    public Guid? TenantId { get; protected set; }

    public int AcademicYear { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public bool IsLocked { get; private set; }

    public IReadOnlyList<TermPeriodDefinition> Periods => _periods;

    public void ChangeIdentity(int academicYear, string code)
    {
        EnsureNotLocked();

        AcademicYear = NormalizeAcademicYear(academicYear);
        Code = NormalizeCode(code);
    }

    public void ChangeName(string name)
    {
        EnsureNotLocked();
        Name = NormalizeName(name);
    }

    public void ChangeDateRange(DateTime startDate, DateTime endDate)
    {
        EnsureNotLocked();

        var normalizedStartDate = startDate.Date;
        var normalizedEndDate = endDate.Date;

        if (normalizedStartDate > normalizedEndDate)
        {
            throw new BusinessException(AcademicTermErrorCodes.InvalidDateRange)
                .WithData(nameof(StartDate), normalizedStartDate)
                .WithData(nameof(EndDate), normalizedEndDate);
        }

        StartDate = normalizedStartDate;
        EndDate = normalizedEndDate;
    }

    public void ReplacePeriods(IEnumerable<TermPeriodDefinition> periods)
    {
        EnsureNotLocked();
        var termPeriodDefinitions = periods as TermPeriodDefinition[] ?? periods.ToArray();
        Check.NotNull(termPeriodDefinitions, nameof(periods));

        var normalizedPeriods = termPeriodDefinitions
            .OrderBy(x => x.PeriodNo)
            .ToList();

        if (normalizedPeriods.Count == 0)
        {
            throw new BusinessException(AcademicTermErrorCodes.PeriodsCannotBeEmpty);
        }

        var seenPeriodNos = new HashSet<int>();
        for (var i = 0; i < normalizedPeriods.Count; i++)
        {
            var currentPeriod = normalizedPeriods[i];
            if (!seenPeriodNos.Add(currentPeriod.PeriodNo))
            {
                throw new BusinessException(AcademicTermErrorCodes.DuplicatePeriodNo)
                    .WithData(nameof(TermPeriodDefinition.PeriodNo), currentPeriod.PeriodNo);
            }

            if (i == 0)
            {
                continue;
            }

            var previousPeriod = normalizedPeriods[i - 1];
            if (currentPeriod.StartTime < previousPeriod.EndTime)
            {
                throw new BusinessException(AcademicTermErrorCodes.InvalidPeriodChronology)
                    .WithData("PreviousPeriodNo", previousPeriod.PeriodNo)
                    .WithData("CurrentPeriodNo", currentPeriod.PeriodNo);
            }
        }

        _periods.Clear();
        _periods.AddRange(normalizedPeriods);
    }

    public void Lock()
    {
        if (IsLocked)
        {
            throw new BusinessException(AcademicTermErrorCodes.TermAlreadyLocked)
                .WithData(nameof(Id), Id);
        }

        IsLocked = true;
    }

    internal static int NormalizeAcademicYear(int academicYear)
    {
        if (academicYear < AcademicTermConsts.MinAcademicYear ||
            academicYear > AcademicTermConsts.MaxAcademicYear)
        {
            throw new BusinessException(AcademicTermErrorCodes.AcademicYearOutOfRange)
                .WithData(nameof(AcademicYear), academicYear);
        }

        return academicYear;
    }

    internal static string NormalizeCode(string code)
    {
        return Check.NotNullOrWhiteSpace(
                code,
                nameof(code),
                AcademicTermConsts.MaxCodeLength
            )
            .Trim();
    }

    private static string NormalizeName(string name)
    {
        return Check.NotNullOrWhiteSpace(
                name,
                nameof(name),
                AcademicTermConsts.MaxNameLength
            )
            .Trim();
    }

    private void EnsureNotLocked()
    {
        if (IsLocked)
        {
            throw new BusinessException(AcademicTermErrorCodes.TermLocked)
                .WithData(nameof(Id), Id);
        }
    }
}
