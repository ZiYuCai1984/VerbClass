using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class CourseOffering : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    protected CourseOffering()
    {
    }

    public CourseOffering(
        Guid id,
        Guid tenantId,
        Guid courseDefinitionId,
        AcademicTerm term,
        DayOfWeek? scheduleDayOfWeek,
        TimeSpan? scheduleStartTime,
        TimeSpan? scheduleEndTime,
        string? scheduleLocation,
        EnrollmentPolicy enrollmentPolicy) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        if (courseDefinitionId == Guid.Empty)
        {
            throw new AbpException("CourseDefinitionId can not be empty.");
        }

        TenantId = tenantId;
        CourseDefinitionId = courseDefinitionId;
        SetTerm(term);
        SetSchedule(
            scheduleDayOfWeek,
            scheduleStartTime,
            scheduleEndTime,
            scheduleLocation
        );
        SetEnrollmentPolicy(enrollmentPolicy);
        Status = CourseOfferingStatus.Draft;
        IsLocked = false;
    }

    public Guid? TenantId { get; protected set; }

    public Guid CourseDefinitionId { get; private set; }

    public int AcademicYear { get; private set; }

    public string TermName { get; private set; } = string.Empty;

    public DayOfWeek? ScheduleDayOfWeek { get; private set; }

    public TimeSpan? ScheduleStartTime { get; private set; }

    public TimeSpan? ScheduleEndTime { get; private set; }

    public string? ScheduleLocation { get; private set; }

    public DateTime EnrollmentStartsAt { get; private set; }

    public DateTime EnrollmentEndsAt { get; private set; }

    public CourseOfferingStatus Status { get; private set; }

    public bool IsLocked { get; private set; }

    public void SetTerm(AcademicTerm term)
    {
        var validatedTerm = Check.NotNull(term, nameof(term));
        AcademicYear = validatedTerm.AcademicYear;
        TermName = validatedTerm.TermName;
    }

    public void SetSchedule(
        DayOfWeek? dayOfWeek,
        TimeSpan? startTime,
        TimeSpan? endTime,
        string? location = null)
    {
        var hasDayOfWeek = dayOfWeek.HasValue;
        var hasStartTime = startTime.HasValue;
        var hasEndTime = endTime.HasValue;
        var hasScheduleValues = hasDayOfWeek || hasStartTime || hasEndTime;

        if (hasScheduleValues && !(hasDayOfWeek && hasStartTime && hasEndTime))
        {
            throw CreateInvalidScheduleException(dayOfWeek, startTime, endTime);
        }

        if (hasScheduleValues)
        {
            var validatedDayOfWeek = dayOfWeek!.Value;
            var validatedStartTime = startTime!.Value;
            var validatedEndTime = endTime!.Value;

            if (!Enum.IsDefined(typeof(DayOfWeek), validatedDayOfWeek) || validatedEndTime <= validatedStartTime)
            {
                throw CreateInvalidScheduleException(dayOfWeek, startTime, endTime);
            }
        }

        ScheduleDayOfWeek = dayOfWeek;
        ScheduleStartTime = startTime;
        ScheduleEndTime = endTime;
        SetScheduleLocation(location);
    }

    public void SetEnrollmentPolicy(EnrollmentPolicy enrollmentPolicy)
    {
        var validatedEnrollmentPolicy = Check.NotNull(enrollmentPolicy, nameof(enrollmentPolicy));
        EnrollmentStartsAt = validatedEnrollmentPolicy.OpensAt;
        EnrollmentEndsAt = validatedEnrollmentPolicy.ClosesAt;
    }

    public AcademicTerm GetTerm()
    {
        return new AcademicTerm(AcademicYear, TermName);
    }

    public ScheduleSlot? GetScheduleSlot()
    {
        if (!ScheduleDayOfWeek.HasValue || !ScheduleStartTime.HasValue || !ScheduleEndTime.HasValue)
        {
            return null;
        }

        return new ScheduleSlot(
            ScheduleDayOfWeek.Value,
            ScheduleStartTime.Value,
            ScheduleEndTime.Value,
            ScheduleLocation
        );
    }

    public EnrollmentPolicy GetEnrollmentPolicy()
    {
        return new EnrollmentPolicy(EnrollmentStartsAt, EnrollmentEndsAt);
    }

    public bool IsEnrollmentOpen(DateTime atTime)
    {
        return atTime >= EnrollmentStartsAt && atTime <= EnrollmentEndsAt;
    }

    public void EnsureAllowsStudentJoin(DateTime atTime)
    {
        if (IsLocked)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingLocked)
                .WithData("CourseOfferingId", Id);
        }

        if (!IsEnrollmentOpen(atTime))
        {
            throw new BusinessException(AcademicErrorCodes.MembershipEnrollmentNotOpen)
                .WithData("CourseOfferingId", Id)
                .WithData("EnrollmentStartsAt", EnrollmentStartsAt)
                .WithData("EnrollmentEndsAt", EnrollmentEndsAt)
                .WithData("AtTime", atTime);
        }
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    private void SetScheduleLocation(string? location)
    {
        if (location.IsNullOrWhiteSpace())
        {
            ScheduleLocation = null;
            return;
        }

        ScheduleLocation = Check.Length(
            location.Trim(),
            nameof(location),
            CourseOfferingConsts.MaxLocationLength
        );
    }

    private static BusinessException CreateInvalidScheduleException(
        DayOfWeek? dayOfWeek,
        TimeSpan? startTime,
        TimeSpan? endTime)
    {
        return new BusinessException(AcademicErrorCodes.CourseOfferingScheduleSlotInvalid)
            .WithData("DayOfWeek", dayOfWeek.HasValue ? (object)(int)dayOfWeek.Value : "(null)")
            .WithData("StartTime", startTime.HasValue ? (object)startTime.Value : "(null)")
            .WithData("EndTime", endTime.HasValue ? (object)endTime.Value : "(null)");
    }
}
