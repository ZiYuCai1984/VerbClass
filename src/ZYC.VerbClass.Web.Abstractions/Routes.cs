namespace ZYC.VerbClass.Web.Abstractions;

// ReSharper disable InconsistentNaming
public static class Routes
{
    private const string admin = nameof(admin);

    public const string Admin_UserManager = $"/{admin}/user-manager";

    public const string Admin_DepartmentManager = $"/{admin}/department-manager";

    public const string Admin_AuditLog = $"/{admin}/audit-log";

    public const string Admin_RoleManager = $"/{admin}/role-manager";

    public const string Admin_Settings = $"/{admin}/settings";


    private const string account = nameof(account);

    public const string Account_Login = $"/{account}/login";

    public const string Account_Logout = $"/{account}/logout";

    public const string Account_UserSettings = $"/{account}/user-settings";

    public const string Account_Avatar = $"/{account}/avatar";

    public const string Account_AccessDenied = $"/{account}/accessdenied";


    private const string academic = nameof(academic);

    public const string Academic_TimeTemplateManager = $"/{academic}/time-template-manager";

    public const string Academic_TermManager = $"/{academic}/term-manager";

    public const string Academic_TimetableManager = $"/{academic}/timetable-manager";


    public const string Academic_CourseDefinitionManager = $"/{academic}/course-definition-manager";

    public const string Academic_CourseOfferingManager = $"/{academic}/course-offering-manager";

    public const string Academic_CourseParticipantManager = $"/{academic}/course-participant-manager";


    private const string debug = nameof(debug);

    public const string Debug_Switch = $"/{debug}/switch-account";
}