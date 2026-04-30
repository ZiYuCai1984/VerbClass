namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

public class CourseParticipantEditorModel : CourseParticipantEditorInput
{
    public Guid? CourseOfferingId { get; set; }

    public CourseParticipantUserOptionModel[] UserOptions { get; set; } = [];

    public CourseParticipantRoleOptionModel[] RoleOptions { get; set; } = [];
}
