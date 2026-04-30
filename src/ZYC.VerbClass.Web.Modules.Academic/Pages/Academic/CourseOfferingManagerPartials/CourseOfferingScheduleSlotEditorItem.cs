using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingScheduleSlotEditorItem
{
    [Display(Name = "Weekday")]
    public AcademicWeekday? Weekday { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Period")]
    public int? PeriodNo { get; set; }
}
