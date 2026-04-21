namespace ZYC.VerbClass.Web.Pages.Admin;

public class RoleManagerContentModel
{
    public RoleListItem[] Roles { get; set; } = [];

    public string? ActiveRoleName { get; set; }

    public RoleInfoModel? SelectedRole { get; set; }

    public RolePermissionEditorModel? Editor { get; set; }

    public string DetailTitle => Editor is not null ? "Edit Permissions" : "Role Details";
}
