namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

public class CourseDefinitionInfoModel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
