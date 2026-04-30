namespace ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;

public class CourseDefinitionCommandResultDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
