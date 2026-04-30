using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.Academic;

public class AcademicPeriodDefinitionDto
{
    [Range(1, int.MaxValue)]
    public int PeriodNo { get; set; }

    [Required]
    [StringLength(AcademicTimeTemplateConsts.MaxPeriodLabelLength)]
    public string Label { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
