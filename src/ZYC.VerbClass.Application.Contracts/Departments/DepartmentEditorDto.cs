namespace ZYC.VerbClass.Application.Contracts.Departments;

public class DepartmentEditorDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public Guid? ParentDepartmentId { get; set; }

    public int Sort { get; set; }

    public bool IsActive { get; set; }

    public bool CanAssignUsers { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }
}
