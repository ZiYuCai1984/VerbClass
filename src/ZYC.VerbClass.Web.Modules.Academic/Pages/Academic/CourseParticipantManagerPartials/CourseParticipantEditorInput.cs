using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

public class CourseParticipantEditorInput : EditorInputBase
{
    [Display(Name = "User")]
    public Guid? UserId { get; set; }

    [EnumDataType(typeof(CourseOfferingParticipantRole))]
    [Display(Name = "Role")]
    public CourseOfferingParticipantRole? Role { get; set; }

    public Guid? ReturnTermId { get; set; }

    public Guid? ReturnOfferingId { get; set; }
}
