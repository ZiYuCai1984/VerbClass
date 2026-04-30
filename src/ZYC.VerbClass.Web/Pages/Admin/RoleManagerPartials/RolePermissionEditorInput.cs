using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Web.Pages.Admin.RoleManagerPartials;

public class RolePermissionEditorInput : EditorInputBase
{
    [Required]
    [Display(Name = "Role")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Permissions")]
    public string[] GrantedPermissionNames { get; set; } = [];
}
