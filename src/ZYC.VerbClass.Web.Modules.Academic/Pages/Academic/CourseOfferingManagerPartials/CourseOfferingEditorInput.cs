using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingEditorInput : EditorInputBase
{
    [Display(Name = "Term")]
    public Guid? AcademicTermId { get; set; }

    [Display(Name = "Course")]
    public Guid? CourseDefinitionId { get; set; }

    [Required]
    [StringLength(CourseOfferingConsts.MaxOfferingCodeLength)]
    [Display(Name = "Offering Code")]
    public string OfferingCode { get; set; } = string.Empty;

    public List<CourseOfferingScheduleSlotEditorItem> ScheduleSlots { get; set; } = [];

    public Guid? ReturnTermId { get; set; }

    public Guid? ReturnOfferingId { get; set; }
}
