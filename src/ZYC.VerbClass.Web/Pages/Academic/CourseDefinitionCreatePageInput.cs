using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseDefinitionCreatePageInput : EditorInputBase
{
    [Required]
    [StringLength(CourseDefinitionConsts.MaxCodeLength)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(CourseDefinitionConsts.MaxNameLength)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(CourseDefinitionConsts.MaxShortNameLength)]
    [Display(Name = "Short name")]
    public string? ShortName { get; set; }

    [StringLength(CourseDefinitionConsts.MaxDescriptionLength)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
