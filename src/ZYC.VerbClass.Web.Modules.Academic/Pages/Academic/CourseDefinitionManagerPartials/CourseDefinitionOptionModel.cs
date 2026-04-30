namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

public class CourseDefinitionOptionModel
{
    public CourseDefinitionOptionModel(Guid id, string code, string name)
    {
        Id = id;
        Code = code;
        Name = name;
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public string DisplayName => $"{Name} ({Code})";
}
