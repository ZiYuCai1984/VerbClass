namespace ZYC.VerbClass.Domain.Shared;

public static class VerbClassRoles
{
    public const string OperationsAdmin = "OperationsAdmin";
    public const string TeachingManager = "TeachingManager";
    public const string Instructor = "Instructor";
    public const string Assistant = "Assistant";
    public const string Student = "Student";

    public static string[] All { get; } =
    [
        OperationsAdmin,
        TeachingManager,
        Instructor,
        Assistant,
        Student
    ];

    public static bool IsManagedRole(string? roleName)
    {
        return Array.Exists(
            All,
            managedRole => string.Equals(managedRole, roleName, StringComparison.OrdinalIgnoreCase)
        );
    }
}
