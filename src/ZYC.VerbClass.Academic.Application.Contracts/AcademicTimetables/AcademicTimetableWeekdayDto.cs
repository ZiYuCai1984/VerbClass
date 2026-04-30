using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;

public class AcademicTimetableWeekdayDto
{
    public AcademicWeekday Weekday { get; set; }

    public string Label { get; set; } = string.Empty;
}
