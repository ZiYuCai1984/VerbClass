using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Application.Contracts.Users;

public abstract class UserInputBase
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Surname { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid[] DepartmentIds { get; set; } = [];

    public Guid? PrimaryDepartmentId { get; set; }

    public string[] RoleNames { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public bool ForcePasswordChangeOnNextLogin { get; set; }
}
