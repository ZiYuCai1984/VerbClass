using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZYC.VerbClass.Academic.Application.Contracts.MyCourses;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Academic;

[Authorize]
public class MyCoursesModel : VerbClassPageModel
{
    private readonly IMyCourseAppService _myCourseAppService;

    public MyCoursesModel(
        ILifetimeScope lifetimeScope,
        IMyCourseAppService myCourseAppService) : base(lifetimeScope)
    {
        _myCourseAppService = myCourseAppService;
    }

    protected override string PageTitle => "My Courses";

    public MyCourseListItemDto[] Courses { get; private set; } = [];

    public int ActiveCourseCount => Courses.Count(x => x.MembershipStatus == CourseMembershipStatus.Active);

    public int PendingCourseCount => Courses.Count(x => x.MembershipStatus == CourseMembershipStatus.Pending);

    public int LockedCourseCount => Courses.Count(x => x.IsLocked);

    public async Task<IActionResult> OnGetAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return Challenge();
        }

        Courses = await _myCourseAppService.GetListAsync();
        return Page();
    }

    public string GetTermText(MyCourseListItemDto course)
    {
        return $"{course.AcademicYear} / {course.TermName}";
    }

    public string GetScheduleTimeText(MyCourseListItemDto course)
    {
        return course.ScheduleDayOfWeek.HasValue && course.ScheduleStartTime.HasValue && course.ScheduleEndTime.HasValue
            ? $"{course.ScheduleDayOfWeek.Value} {course.ScheduleStartTime.Value:hh\\:mm}-{course.ScheduleEndTime.Value:hh\\:mm}"
            : "Schedule not set";
    }

    public string GetScheduleLocationText(MyCourseListItemDto course)
    {
        return string.IsNullOrWhiteSpace(course.ScheduleLocation)
            ? "Location not set"
            : course.ScheduleLocation;
    }

    public string GetOfferingStatusText(CourseOfferingStatus status)
    {
        return status.ToString();
    }

    public string GetOfferingStatusTagClass(CourseOfferingStatus status)
    {
        return status switch
        {
            CourseOfferingStatus.Published => "is-success",
            CourseOfferingStatus.Active => "is-success",
            CourseOfferingStatus.Draft => "is-warning",
            CourseOfferingStatus.Closed => "is-danger",
            CourseOfferingStatus.Cancelled => "is-danger",
            _ => string.Empty
        };
    }

    public string GetMembershipRoleText(CourseMembershipRole role)
    {
        return role switch
        {
            CourseMembershipRole.TeachingAssistant => "Teaching Assistant",
            _ => role.ToString()
        };
    }

    public string GetMembershipStatusText(CourseMembershipStatus status)
    {
        return status.ToString();
    }

    public string GetMembershipStatusTagClass(CourseMembershipStatus status)
    {
        return status switch
        {
            CourseMembershipStatus.Active => "is-success",
            CourseMembershipStatus.Pending => "is-warning",
            CourseMembershipStatus.Dropped => "is-danger",
            CourseMembershipStatus.Removed => "is-danger",
            _ => string.Empty
        };
    }
}
