namespace ZYC.VerbClass.Web.Pages.Admin.UserManagerPartials;

public class UserManagerContentModel
{
    public UserListItem[] Users { get; set; } = [];

    public Guid? ActiveUserId { get; set; }

    public UserInfoModel? SelectedUser { get; set; }

    public UserEditorModel? Editor { get; set; }

    public bool CanCreate { get; set; }

    public string DetailTitle
    {
        get
        {
            if (Editor?.IsCreate == true)
            {
                return "New User";
            }

            if (Editor is not null)
            {
                return "Edit User";
            }

            return "User Details";
        }
    }
}