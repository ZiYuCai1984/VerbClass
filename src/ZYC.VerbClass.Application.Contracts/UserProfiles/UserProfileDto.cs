namespace ZYC.VerbClass.Application.Contracts.UserProfiles;

public class UserProfileDto
{
    public const string DefaultDisplayName = "User";

    public const string DefaultAvatarVersion = "default";

    public string DisplayName { get; set; } = DefaultDisplayName;

    public string UserNameDisplay { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool HasCustomAvatar { get; set; }

    public string AvatarVersion { get; set; } = DefaultAvatarVersion;

    public string[] Roles { get; set; } = [];
}