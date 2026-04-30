using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;

[Audited]
public class AcademicTimeTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<TimeTemplatePeriodDefinition> _periods = [];

    protected AcademicTimeTemplate()
    {
    }

    public AcademicTimeTemplate(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        IEnumerable<TimeTemplatePeriodDefinition> periods) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        ChangeCode(code);
        ChangeName(name);
        ReplacePeriods(periods);
    }

    public Guid? TenantId { get; protected set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyList<TimeTemplatePeriodDefinition> Periods => _periods;

    public void ChangeCode(string code)
    {
        Code = NormalizeCode(code);
    }

    public void ChangeName(string name)
    {
        Name = NormalizeName(name);
    }

    public void ReplacePeriods(IEnumerable<TimeTemplatePeriodDefinition> periods)
    {
        var timeTemplatePeriods = periods as TimeTemplatePeriodDefinition[] ?? periods.ToArray();
        Check.NotNull(timeTemplatePeriods, nameof(periods));

        var normalizedPeriods = timeTemplatePeriods
            .OrderBy(x => x.PeriodNo)
            .ToList();

        if (normalizedPeriods.Count == 0)
        {
            throw new BusinessException(AcademicTimeTemplateErrorCodes.PeriodsCannotBeEmpty);
        }

        var seenPeriodNos = new HashSet<int>();
        for (var i = 0; i < normalizedPeriods.Count; i++)
        {
            var currentPeriod = normalizedPeriods[i];
            if (!seenPeriodNos.Add(currentPeriod.PeriodNo))
            {
                throw new BusinessException(AcademicTimeTemplateErrorCodes.DuplicatePeriodNo)
                    .WithData(nameof(TimeTemplatePeriodDefinition.PeriodNo), currentPeriod.PeriodNo);
            }

            if (i == 0)
            {
                continue;
            }

            var previousPeriod = normalizedPeriods[i - 1];
            if (currentPeriod.StartTime < previousPeriod.EndTime)
            {
                throw new BusinessException(AcademicTimeTemplateErrorCodes.InvalidPeriodChronology)
                    .WithData("PreviousPeriodNo", previousPeriod.PeriodNo)
                    .WithData("CurrentPeriodNo", currentPeriod.PeriodNo);
            }
        }

        _periods.Clear();
        _periods.AddRange(normalizedPeriods);
    }

    internal static string NormalizeCode(string code)
    {
        return Check.NotNullOrWhiteSpace(
                code,
                nameof(code),
                AcademicTimeTemplateConsts.MaxCodeLength
            )
            .Trim();
    }

    private static string NormalizeName(string name)
    {
        return Check.NotNullOrWhiteSpace(
                name,
                nameof(name),
                AcademicTimeTemplateConsts.MaxNameLength
            )
            .Trim();
    }
}
