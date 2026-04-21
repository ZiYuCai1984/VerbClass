namespace ZYC.VerbClass.Web.Pages.Admin;

public class UserEditorModel : UserEditorInput
{
    public bool IsCreate { get; set; }

    public Guid? UserId { get; set; }

    public bool CanAssignRoles { get; set; }

    public List<UserDepartmentOption> DepartmentOptions { get; } = [];

    public List<string> RoleOptions { get; } = [];

    public string SubmitText => IsCreate ? "Create User" : "Save Changes";
}
