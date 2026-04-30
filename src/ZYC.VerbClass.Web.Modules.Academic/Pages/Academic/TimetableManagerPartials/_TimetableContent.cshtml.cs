using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

public class TimetableContentModel
{
    public CourseOfferingTermOptionModel[] Terms { get; set; } = [];

    public Guid? ActiveTermId { get; set; }

    public TimetableViewModel? Timetable { get; set; }
}
