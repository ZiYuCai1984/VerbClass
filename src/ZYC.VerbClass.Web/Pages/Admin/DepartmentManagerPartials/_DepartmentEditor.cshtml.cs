namespace ZYC.VerbClass.Web.Pages.Admin.DepartmentManagerPartials;

public class DepartmentEditorModel : DepartmentEditorInput
{
    public bool IsCreate { get; set; }

    public Guid? DepartmentId { get; set; }

    public List<DepartmentParentOption> ParentOptions { get; } = [];

    public string SubmitText => IsCreate ? "Create Department" : "Save Changes";
}
