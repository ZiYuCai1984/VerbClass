namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

public record TermListItem(
    Guid Id,
    int AcademicYear,
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsLocked,
    int PeriodCount
)
{
    public string Title => $"{AcademicYear} {Name}";

    public string DateRangeDisplay => $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}";
}
