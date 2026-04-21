using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public abstract class CourseOfferingInputBase
{
    public Guid CourseDefinitionId { get; set; }

    [Range(CourseOfferingConsts.MinAcademicYear, CourseOfferingConsts.MaxAcademicYear)]
    public int AcademicYear { get; set; }

    [Required]
    [StringLength(CourseOfferingConsts.MaxTermNameLength)]
    public string TermName { get; set; } = string.Empty;

    public DayOfWeek? ScheduleDayOfWeek { get; set; }

    public TimeSpan? ScheduleStartTime { get; set; }

    public TimeSpan? ScheduleEndTime { get; set; }

    [StringLength(CourseOfferingConsts.MaxLocationLength)]
    public string? ScheduleLocation { get; set; }

    public DateTime EnrollmentStartsAt { get; set; } = DateTime.Today;

    public DateTime EnrollmentEndsAt { get; set; } = DateTime.Today.AddDays(7);
}
