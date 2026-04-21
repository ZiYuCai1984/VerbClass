namespace ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;

public class CourseDefinitionListItemDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
