using System.Reflection;

namespace ZYC.VerbClass.Domain.Shared;

public static class VerbClassPermissions
{
    public const string GroupName = "VerbClass";

    public static class Users
    {
        private const string Prefix = GroupName + "." + nameof(Users);
        public const string Access = Prefix + "." + nameof(Access);
        public const string Create = Prefix + "." + nameof(Create);
        public const string Update = Prefix + "." + nameof(Update);
        public const string Delete = Prefix + "." + nameof(Delete);
        public const string AssignRoles = Prefix + "." + nameof(AssignRoles);
    }

    public static class Departments
    {
        private const string Prefix = GroupName + "." + nameof(Departments);
        public const string Access = Prefix + "." + nameof(Access);
        public const string Create = Prefix + "." + nameof(Create);
        public const string Update = Prefix + "." + nameof(Update);
        public const string Delete = Prefix + "." + nameof(Delete);
    }

    public static class AuditLogs
    {
        private const string Prefix = GroupName + "." + nameof(AuditLogs);
        public const string Access = Prefix + "." + nameof(Access);
    }

    public static class UserSettings
    {
        private const string Prefix = GroupName + "." + nameof(UserSettings);
        public const string Access = Prefix + "." + nameof(Access);
        public const string Update = Prefix + "." + nameof(Update);
    }

    public static class Roles
    {
        private const string Prefix = GroupName + "." + nameof(Roles);
        public const string Access = Prefix + "." + nameof(Access);
        public const string ManagePermissions = Prefix + "." + nameof(ManagePermissions);
    }


    public static (string Name, string DisplayName)[] GetPermissions()
    {
        var rootType = typeof(VerbClassPermissions);
        var result = new List<(string Name, string DisplayName)>();

        foreach (var moduleType in rootType.GetNestedTypes(BindingFlags.Public))
        {
            var permissionFields = moduleType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(x =>
                    x.IsLiteral &&
                    !x.IsInitOnly &&
                    x.FieldType == typeof(string));

            foreach (var field in permissionFields)
            {
                var permissionName = field.GetRawConstantValue() as string;
                if (string.IsNullOrWhiteSpace(permissionName))
                {
                    continue;
                }

                result.Add((
                    permissionName,
                    $"Permission:{moduleType.Name}.{field.Name}"
                ));
            }
        }

        return result.ToArray();
    }
}
