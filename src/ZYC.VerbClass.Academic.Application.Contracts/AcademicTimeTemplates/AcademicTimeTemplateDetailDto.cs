using ZYC.VerbClass.Academic.Application.Contracts.Academic;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;

public class AcademicTimeTemplateDetailDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public AcademicPeriodDefinitionDto[] Periods { get; set; } = [];
}
