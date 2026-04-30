using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

public class CourseParticipantRoleOptionModel
{
    public CourseParticipantRoleOptionModel(CourseOfferingParticipantRole value, string label)
    {
        Value = value;
        Label = label;
    }

    public CourseOfferingParticipantRole Value { get; }

    public string Label { get; }
}
