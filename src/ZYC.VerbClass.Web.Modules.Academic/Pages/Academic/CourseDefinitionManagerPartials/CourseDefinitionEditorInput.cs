using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

public class CourseDefinitionEditorInput : EditorInputBase
{
    [Required]
    [StringLength(CourseDefinitionConsts.MaxCodeLength)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(CourseDefinitionConsts.MaxNameLength)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(CourseDefinitionConsts.MaxDescriptionLength)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public Guid? ReturnCourseDefinitionId { get; set; }
}
