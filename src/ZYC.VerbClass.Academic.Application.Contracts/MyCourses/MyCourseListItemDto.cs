using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.MyCourses;

public class MyCourseListItemDto
{
    public Guid CourseOfferingId { get; set; }

    public Guid CourseDefinitionId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public string? CourseShortName { get; set; }

    public int AcademicYear { get; set; }

    public string TermName { get; set; } = string.Empty;

    public DayOfWeek? ScheduleDayOfWeek { get; set; }

    public TimeSpan? ScheduleStartTime { get; set; }

    public TimeSpan? ScheduleEndTime { get; set; }

    public string? ScheduleLocation { get; set; }

    public CourseOfferingStatus OfferingStatus { get; set; }

    public bool IsLocked { get; set; }

    public CourseMembershipRole MembershipRole { get; set; }

    public CourseMembershipStatus MembershipStatus { get; set; }
}
