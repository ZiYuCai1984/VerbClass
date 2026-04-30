using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

public class CourseParticipantManagerContentModel
{
    public CourseOfferingTermOptionModel[] Terms { get; set; } = [];

    public Guid? ActiveTermId { get; set; }

    public CourseOfferingListItem[] Offerings { get; set; } = [];

    public Guid? ActiveOfferingId { get; set; }

    public CourseParticipantDetailModel? Detail { get; set; }

    public string DetailTitle => Detail?.OfferingTitle ?? "Course Participants";
}
