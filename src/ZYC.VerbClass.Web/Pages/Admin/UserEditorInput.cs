using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Web.Pages.Admin;

public class UserEditorInput : EditorInputBase
{
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Family name")]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Given name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone] [Display(Name = "Phone")] public string? PhoneNumber { get; set; }

    [Display(Name = "Departments")] public Guid[] DepartmentIds { get; set; } = [];

    [Display(Name = "Primary department")] public Guid? PrimaryDepartmentId { get; set; }

    [Display(Name = "Roles")] public string[] RoleNames { get; set; } = [];

    [Display(Name = "Active")] public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    [Display(Name = "Initial password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Require password change on next sign-in")]
    public bool ForcePasswordChangeOnNextLogin { get; set; }

    public Guid? ReturnUserId { get; set; }
}
