using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

public class TimeTemplateInfoModel
{
    public Guid Id { get; init; }

    public string Code { get; init; } = "";

    public string Name { get; init; } = "";

    public PeriodRowModel[] Periods { get; init; } = [];
}
