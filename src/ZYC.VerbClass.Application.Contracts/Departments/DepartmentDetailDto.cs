namespace ZYC.VerbClass.Application.Contracts.Departments;

public class DepartmentDetailDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string? ParentDepartmentName { get; set; }

    public string PathDisplay { get; set; } = "/";

    public int Sort { get; set; }

    public bool IsActive { get; set; }

    public bool CanAssignUsers { get; set; }

    public int CurrentUserCount { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public bool HasChildren { get; set; }

    public bool CanCreateChild { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }
}
