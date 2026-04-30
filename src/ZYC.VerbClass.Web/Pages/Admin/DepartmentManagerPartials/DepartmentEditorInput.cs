using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Admin.DepartmentManagerPartials;

public class DepartmentEditorInput : EditorInputBase
{
    [Required]
    [StringLength(DepartmentConsts.MaxCodeLength)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(DepartmentConsts.MaxNameLength)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(DepartmentConsts.MaxShortNameLength)]
    [Display(Name = "Short name")]
    public string? ShortName { get; set; }

    [Display(Name = "Parent department")]
    public Guid? ParentDepartmentId { get; set; }

    [Display(Name = "Sort")]
    public int Sort { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Allow user assignment")]
    public bool CanAssignUsers { get; set; } = true;

    [DataType(DataType.Date)]
    [Display(Name = "Effective from")]
    public DateTime? EffectiveFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Effective to")]
    public DateTime? EffectiveTo { get; set; }

    public Guid? ReturnDepartmentId { get; set; }

}
