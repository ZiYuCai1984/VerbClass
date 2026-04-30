namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;

public class AcademicTimeTemplateListItemDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int PeriodCount { get; set; }
}
