using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;

public abstract class AcademicTimeTemplateInputBase
{
    [Required]
    [StringLength(AcademicTimeTemplateConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(AcademicTimeTemplateConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public AcademicPeriodDefinitionDto[] Periods { get; set; } = [];
}
