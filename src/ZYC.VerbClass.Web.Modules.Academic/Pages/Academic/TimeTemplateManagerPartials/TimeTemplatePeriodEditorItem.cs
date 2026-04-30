using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

public class TimeTemplatePeriodEditorItem
{
    [Range(1, int.MaxValue)]
    public int PeriodNo { get; set; }

    [Required]
    [StringLength(AcademicTimeTemplateConsts.MaxPeriodLabelLength)]
    public string Label { get; set; } = string.Empty;

    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;
}
