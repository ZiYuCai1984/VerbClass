using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

[Audited]
public class CourseOffering : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<CourseOfferingScheduleSlot> _scheduleSlots = [];

    protected CourseOffering()
    {
    }

    public CourseOffering(
        Guid id,
        Guid tenantId,
        Guid academicTermId,
        Guid courseDefinitionId,
        string offeringCode,
        string courseCodeSnapshot,
        string courseNameSnapshot,
        IEnumerable<CourseOfferingScheduleSlot> scheduleSlots,
        IEnumerable<int> validPeriodNos) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        if (academicTermId == Guid.Empty)
        {
            throw new AbpException("AcademicTermId can not be empty.");
        }

        if (courseDefinitionId == Guid.Empty)
        {
            throw new AbpException("CourseDefinitionId can not be empty.");
        }

        TenantId = tenantId;
        AcademicTermId = academicTermId;
        CourseDefinitionId = courseDefinitionId;
        ChangeOfferingCode(offeringCode);
        CourseCodeSnapshot = NormalizeCourseCodeSnapshot(courseCodeSnapshot);
        CourseNameSnapshot = NormalizeCourseNameSnapshot(courseNameSnapshot);
        ReplaceScheduleSlots(scheduleSlots, validPeriodNos);
    }

    public Guid? TenantId { get; protected set; }

    public Guid AcademicTermId { get; private set; }

    public Guid CourseDefinitionId { get; private set; }

    public string OfferingCode { get; private set; } = string.Empty;

    public string CourseCodeSnapshot { get; private set; } = string.Empty;

    public string CourseNameSnapshot { get; private set; } = string.Empty;

    public IReadOnlyList<CourseOfferingScheduleSlot> ScheduleSlots => _scheduleSlots;

    public void ChangeOfferingCode(string offeringCode)
    {
        OfferingCode = NormalizeOfferingCode(offeringCode);
    }

    public void ReplaceScheduleSlots(
        IEnumerable<CourseOfferingScheduleSlot> scheduleSlots,
        IEnumerable<int> validPeriodNos)
    {
        Check.NotNull(scheduleSlots, nameof(scheduleSlots));
        Check.NotNull(validPeriodNos, nameof(validPeriodNos));

        var validPeriodNoSet = validPeriodNos.ToHashSet();
        var normalizedSlots = scheduleSlots
            .OrderBy(x => x.Weekday)
            .ThenBy(x => x.PeriodNo)
            .ToArray();

        var seenSlots = new HashSet<(AcademicWeekday Weekday, int PeriodNo)>();
        foreach (var slot in normalizedSlots)
        {
            CourseOfferingScheduleSlot.NormalizeWeekday(slot.Weekday);
            CourseOfferingScheduleSlot.NormalizePeriodNo(slot.PeriodNo);

            if (!validPeriodNoSet.Contains(slot.PeriodNo))
            {
                throw new BusinessException(CourseOfferingErrorCodes.UnknownTermPeriodNo)
                    .WithData(nameof(CourseOfferingScheduleSlot.PeriodNo), slot.PeriodNo);
            }

            var key = (slot.Weekday, slot.PeriodNo);
            if (!seenSlots.Add(key))
            {
                throw new BusinessException(CourseOfferingErrorCodes.DuplicateScheduleSlot)
                    .WithData(nameof(CourseOfferingScheduleSlot.Weekday), slot.Weekday)
                    .WithData(nameof(CourseOfferingScheduleSlot.PeriodNo), slot.PeriodNo);
            }
        }

        _scheduleSlots.Clear();
        _scheduleSlots.AddRange(normalizedSlots);
    }

    internal static string NormalizeOfferingCode(string offeringCode)
    {
        return Check.NotNullOrWhiteSpace(
                offeringCode,
                nameof(offeringCode),
                CourseOfferingConsts.MaxOfferingCodeLength
            )
            .Trim();
    }

    private static string NormalizeCourseCodeSnapshot(string courseCodeSnapshot)
    {
        return Check.NotNullOrWhiteSpace(
                courseCodeSnapshot,
                nameof(courseCodeSnapshot),
                CourseOfferingConsts.MaxCourseCodeSnapshotLength
            )
            .Trim();
    }

    private static string NormalizeCourseNameSnapshot(string courseNameSnapshot)
    {
        return Check.NotNullOrWhiteSpace(
                courseNameSnapshot,
                nameof(courseNameSnapshot),
                CourseOfferingConsts.MaxCourseNameSnapshotLength
            )
            .Trim();
    }
}
