namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;

public class AcademicTimetablePeriodDto
{
    public int PeriodNo { get; set; }

    public string Label { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
