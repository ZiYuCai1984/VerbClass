namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

public record TemplateOptionModel(
    Guid Id,
    string Code,
    string Name,
    PeriodRowModel[] Periods
);
