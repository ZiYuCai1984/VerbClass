using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseOfferingListItemModel
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

    public CourseOfferingStatus Status { get; set; }

    public bool IsLocked { get; set; }

    public bool HasSchedule => ScheduleDayOfWeek.HasValue && ScheduleStartTime.HasValue && ScheduleEndTime.HasValue;

    public string TermLabel => $"{AcademicYear} / {TermName}";

    public string ScheduleLabel => HasSchedule
        ? $"{ScheduleDayOfWeek!.Value} {ScheduleStartTime!.Value:hh\\:mm}-{ScheduleEndTime!.Value:hh\\:mm}"
        : "Schedule not set";

    public string LocationLabel => string.IsNullOrWhiteSpace(ScheduleLocation)
        ? "Location not set"
        : ScheduleLocation;
}
