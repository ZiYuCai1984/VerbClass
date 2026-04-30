namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingListItem
{
    public CourseOfferingListItem(
        Guid id,
        Guid academicTermId,
        string offeringCode,
        string courseCodeSnapshot,
        string courseNameSnapshot,
        string scheduleSummary)
    {
        Id = id;
        AcademicTermId = academicTermId;
        OfferingCode = offeringCode;
        CourseCodeSnapshot = courseCodeSnapshot;
        CourseNameSnapshot = courseNameSnapshot;
        ScheduleSummary = scheduleSummary;
    }

    public Guid Id { get; }

    public Guid AcademicTermId { get; }

    public string OfferingCode { get; }

    public string CourseCodeSnapshot { get; }

    public string CourseNameSnapshot { get; }

    public string ScheduleSummary { get; }

    public string Title => $"{OfferingCode} {CourseNameSnapshot}";
}
