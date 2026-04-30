namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingInfoModel
{
    public Guid Id { get; set; }

    public Guid AcademicTermId { get; set; }

    public string AcademicTermTitle { get; set; } = string.Empty;

    public Guid CourseDefinitionId { get; set; }

    public string OfferingCode { get; set; } = string.Empty;

    public string CourseCodeSnapshot { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;

    public CourseOfferingScheduleRowModel[] ScheduleSlots { get; set; } = [];

    public string Title => $"{OfferingCode} {CourseNameSnapshot}";
}
