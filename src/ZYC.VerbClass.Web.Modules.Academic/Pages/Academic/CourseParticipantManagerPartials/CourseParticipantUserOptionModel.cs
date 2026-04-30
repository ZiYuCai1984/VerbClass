namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseParticipantManagerPartials;

public class CourseParticipantUserOptionModel
{
    public CourseParticipantUserOptionModel(
        Guid userId,
        string displayName,
        string userName,
        string email,
        string[] roleNames)
    {
        UserId = userId;
        DisplayName = displayName;
        UserName = userName;
        Email = email;
        RoleNames = roleNames;
    }

    public Guid UserId { get; }

    public string DisplayName { get; }

    public string UserName { get; }

    public string Email { get; }

    public string[] RoleNames { get; }

    public string DisplayText
    {
        get
        {
            var roles = RoleNames.Length == 0
                ? "No role"
                : string.Join(", ", RoleNames);

            return $"{DisplayName} ({UserName}) - {roles}";
        }
    }
}
