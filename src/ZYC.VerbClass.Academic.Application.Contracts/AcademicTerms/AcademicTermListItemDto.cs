namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;

public class AcademicTermListItemDto
{
    public Guid Id { get; set; }

    public int AcademicYear { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsLocked { get; set; }

    public int PeriodCount { get; set; }
}
