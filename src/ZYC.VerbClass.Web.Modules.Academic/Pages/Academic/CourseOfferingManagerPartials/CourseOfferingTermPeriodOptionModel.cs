namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingTermPeriodOptionModel
{
    public CourseOfferingTermPeriodOptionModel(
        int periodNo,
        string label,
        string startTime,
        string endTime)
    {
        PeriodNo = periodNo;
        Label = label;
        StartTime = startTime;
        EndTime = endTime;
    }

    public int PeriodNo { get; }

    public string Label { get; }

    public string StartTime { get; }

    public string EndTime { get; }

    public string DisplayName => $"{PeriodNo} - {Label} ({StartTime}-{EndTime})";
}
