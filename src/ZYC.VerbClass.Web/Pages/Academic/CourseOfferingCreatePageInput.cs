using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseOfferingCreatePageInput : EditorInputBase
{
    [Required]
    [Display(Name = "Course definition")]
    public Guid? CourseDefinitionId { get; set; }

    [Range(CourseOfferingConsts.MinAcademicYear, CourseOfferingConsts.MaxAcademicYear)]
    [Display(Name = "Academic year")]
    public int AcademicYear { get; set; } = DateTime.Today.Year;

    [Required]
    [StringLength(CourseOfferingConsts.MaxTermNameLength)]
    [Display(Name = "Term")]
    public string TermName { get; set; } = string.Empty;

    [Display(Name = "Day of week")]
    public DayOfWeek? ScheduleDayOfWeek { get; set; }

    [Display(Name = "Start time")]
    public TimeSpan? ScheduleStartTime { get; set; }

    [Display(Name = "End time")]
    public TimeSpan? ScheduleEndTime { get; set; }

    [StringLength(CourseOfferingConsts.MaxLocationLength)]
    [Display(Name = "Location")]
    public string? ScheduleLocation { get; set; }

    [Display(Name = "Enrollment opens at")]
    public DateTime EnrollmentStartsAt { get; set; } = DateTime.Today;

    [Display(Name = "Enrollment closes at")]
    public DateTime EnrollmentEndsAt { get; set; } = DateTime.Today.AddDays(7);
}
