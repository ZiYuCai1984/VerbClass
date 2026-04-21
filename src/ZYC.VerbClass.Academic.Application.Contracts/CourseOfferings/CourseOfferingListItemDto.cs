using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public class CourseOfferingListItemDto
{
    public Guid Id { get; set; }

    public Guid CourseDefinitionId { get; set; }

    public string CourseDefinitionCode { get; set; } = string.Empty;

    public string CourseDefinitionName { get; set; } = string.Empty;

    public int AcademicYear { get; set; }

    public string TermName { get; set; } = string.Empty;

    public DayOfWeek? ScheduleDayOfWeek { get; set; }

    public TimeSpan? ScheduleStartTime { get; set; }

    public TimeSpan? ScheduleEndTime { get; set; }

    public string? ScheduleLocation { get; set; }

    public DateTime EnrollmentStartsAt { get; set; }

    public DateTime EnrollmentEndsAt { get; set; }

    public CourseOfferingStatus Status { get; set; }

    public bool IsLocked { get; set; }
}
