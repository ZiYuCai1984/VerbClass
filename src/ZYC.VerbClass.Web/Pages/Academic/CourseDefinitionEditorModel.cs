namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseDefinitionEditorModel : CourseDefinitionCreatePageInput
{
    public bool IsCreate { get; set; }

    public Guid? CourseDefinitionId { get; set; }

    public Guid? ReturnCourseDefinitionId { get; set; }

    public string SubmitText => IsCreate
        ? "Create Course Definition"
        : "Save Changes";
}
