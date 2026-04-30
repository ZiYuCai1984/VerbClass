namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingScheduleRowModel
{
    public CourseOfferingScheduleRowModel(
        string weekday,
        int periodNo,
        string periodLabel,
        string startTime,
        string endTime)
    {
        Weekday = weekday;
        PeriodNo = periodNo;
        PeriodLabel = periodLabel;
        StartTime = startTime;
        EndTime = endTime;
    }

    public string Weekday { get; }

    public int PeriodNo { get; }

    public string PeriodLabel { get; }

    public string StartTime { get; }

    public string EndTime { get; }
}
