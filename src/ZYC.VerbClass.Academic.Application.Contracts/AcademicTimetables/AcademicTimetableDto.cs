namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;

public class AcademicTimetableDto
{
    public Guid AcademicTermId { get; set; }

    public int AcademicYear { get; set; }

    public string AcademicTermCode { get; set; } = string.Empty;

    public string AcademicTermName { get; set; } = string.Empty;

    public AcademicTimetablePeriodDto[] Periods { get; set; } = [];

    public AcademicTimetableWeekdayDto[] Weekdays { get; set; } = [];

    public AcademicTimetableCellDto[] Cells { get; set; } = [];

    public AcademicTimetableOfferingDto[] UnscheduledOfferings { get; set; } = [];
}
