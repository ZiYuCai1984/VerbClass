using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

public class TimeTemplateEditorInput : EditorInputBase
{
    [Required]
    [StringLength(AcademicTimeTemplateConsts.MaxCodeLength)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(AcademicTimeTemplateConsts.MaxNameLength)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    public List<TimeTemplatePeriodEditorItem> Periods { get; set; } = [];

    public Guid? ReturnTemplateId { get; set; }
}
