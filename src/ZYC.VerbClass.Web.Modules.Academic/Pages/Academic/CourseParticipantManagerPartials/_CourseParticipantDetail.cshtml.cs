namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

public class CourseParticipantDetailModel
{
    public Guid CourseOfferingId { get; set; }

    public Guid AcademicTermId { get; set; }

    public string OfferingTitle { get; set; } = string.Empty;

    public CourseParticipantListItemModel[] Participants { get; set; } = [];

    public CourseParticipantEditorModel Editor { get; set; } = new();

    public CourseParticipantRoleOptionModel[] RoleOptions { get; set; } = [];
}
