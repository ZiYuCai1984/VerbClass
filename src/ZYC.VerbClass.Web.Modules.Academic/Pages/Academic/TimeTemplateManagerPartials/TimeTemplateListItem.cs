namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

public record TimeTemplateListItem(
    Guid Id,
    string Code,
    string Name,
    int PeriodCount
);
