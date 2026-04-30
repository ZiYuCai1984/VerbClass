using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;

public class AcademicTimetableCellDto
{
    public AcademicWeekday Weekday { get; set; }

    public int PeriodNo { get; set; }

    public AcademicTimetableOfferingDto[] Offerings { get; set; } = [];
}
