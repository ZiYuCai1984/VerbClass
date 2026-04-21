using ZYC.VerbClass.Application.Contracts.Roles;

namespace ZYC.VerbClass.Web.Pages.Admin;

public class RolePermissionEditorModel : RolePermissionEditorInput
{
    public RolePermissionItemDto[] Permissions { get; set; } = [];

    public string SubmitText => "Save Permissions";
}
