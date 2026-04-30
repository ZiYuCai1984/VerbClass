namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

public class TimetableOfferingModel
{
    public Guid Id { get; set; }

    public string OfferingCode { get; set; } = string.Empty;

    public string CourseCodeSnapshot { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;

    public string TeacherLine { get; set; } = string.Empty;

    public string OfferingUrl { get; set; } = string.Empty;
}
