namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

public class TermInfoModel
{
    public Guid Id { get; init; }

    public int AcademicYear { get; init; }

    public string Code { get; init; } = "";

    public string Name { get; init; } = "";

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public bool IsLocked { get; init; }

    public PeriodRowModel[] Periods { get; init; } = [];

    public string Title => $"{AcademicYear} {Name}";
}
