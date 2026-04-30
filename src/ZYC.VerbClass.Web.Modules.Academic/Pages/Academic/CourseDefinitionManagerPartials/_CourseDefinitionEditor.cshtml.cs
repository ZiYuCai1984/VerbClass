namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

public class CourseDefinitionEditorModel : CourseDefinitionEditorInput
{
    public bool IsCreate { get; set; }

    public Guid? CourseDefinitionId { get; set; }

    public bool IsActive { get; set; }

    public string SubmitText => IsCreate ? "Create Course" : "Save Changes";

    public string DetailTitle => IsCreate ? "New Course" : "Edit Course";
}
