namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

public class TimetableRowModel
{
    public TimetablePeriodModel Period { get; set; } = new();

    public TimetableCellModel[] Cells { get; set; } = [];
}
