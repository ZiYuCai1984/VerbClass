using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

public class TimetableCellModel
{
    public AcademicWeekday Weekday { get; set; }

    public int PeriodNo { get; set; }

    public TimetableOfferingModel[] Offerings { get; set; } = [];
}