using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Application.Contracts.Departments;

public abstract class DepartmentInputBase
{
    [Required]
    [StringLength(DepartmentConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(DepartmentConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(DepartmentConsts.MaxShortNameLength)]
    public string? ShortName { get; set; }

    public Guid? ParentDepartmentId { get; set; }

    public int Sort { get; set; }

    public bool IsActive { get; set; } = true;

    public bool CanAssignUsers { get; set; } = true;

    [DataType(DataType.Date)]
    public DateTime? EffectiveFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EffectiveTo { get; set; }
}
