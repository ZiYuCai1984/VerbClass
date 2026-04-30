using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

public class TermEditorInput : EditorInputBase
{
    [Required]
    [Range(AcademicTermConsts.MinAcademicYear, AcademicTermConsts.MaxAcademicYear)]
    [Display(Name = "Academic Year")]
    public int? AcademicYear { get; set; }

    [Required]
    [StringLength(AcademicTermConsts.MaxCodeLength)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(AcademicTermConsts.MaxNameLength)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateOnly? StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateOnly? EndDate { get; set; }

    [Display(Name = "Template")]
    public Guid? TimeTemplateId { get; set; }

    public Guid? ReturnTermId { get; set; }
}
