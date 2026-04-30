namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

public class CourseDefinitionListItem
{
    public CourseDefinitionListItem(
        Guid id,
        string code,
        string name,
        bool isActive)
    {
        Id = id;
        Code = code;
        Name = name;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public string Code { get; }

    public string Name { get; }

    public bool IsActive { get; }
}
