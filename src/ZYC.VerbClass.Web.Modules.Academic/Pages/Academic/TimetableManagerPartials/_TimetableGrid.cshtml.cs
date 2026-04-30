namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

public class TimetableViewModel
{
    public Guid AcademicTermId { get; set; }

    public string TermTitle { get; set; } = string.Empty;

    public string[] WeekdayLabels { get; set; } = [];

    public TimetableRowModel[] Rows { get; set; } = [];

    public TimetableOfferingModel[] UnscheduledOfferings { get; set; } = [];

    public int ScheduledOfferingCount { get; set; }
}
