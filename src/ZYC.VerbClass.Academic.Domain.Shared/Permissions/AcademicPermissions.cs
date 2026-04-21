using System.Reflection;

namespace ZYC.VerbClass.Academic.Domain.Shared.Permissions;

public static class AcademicPermissions
{
    public const string GroupName = AcademicConsts.ModuleName;

    public static class Module
    {
        private const string Prefix = GroupName + "." + nameof(Module);

        public const string Access = Prefix + "." + nameof(Access);
    }

    public static class CourseDefinitions
    {
        private const string Prefix = GroupName + "." + nameof(CourseDefinitions);

        public const string Access = Prefix + "." + nameof(Access);
        public const string Create = Prefix + "." + nameof(Create);
        public const string Update = Prefix + "." + nameof(Update);
    }

    public static class CourseOfferings
    {
        private const string Prefix = GroupName + "." + nameof(CourseOfferings);

        public const string Access = Prefix + "." + nameof(Access);
        public const string Create = Prefix + "." + nameof(Create);
        public const string Lock = Prefix + "." + nameof(Lock);
        public const string Unlock = Prefix + "." + nameof(Unlock);
    }

    public static class Memberships
    {
        private const string Prefix = GroupName + "." + nameof(Memberships);

        public const string Access = Prefix + "." + nameof(Access);
        public const string Join = Prefix + "." + nameof(Join);
        public const string Drop = Prefix + "." + nameof(Drop);
        public const string UpdateRole = Prefix + "." + nameof(UpdateRole);
    }

    public static class Schedules
    {
        private const string Prefix = GroupName + "." + nameof(Schedules);

        public const string Access = Prefix + "." + nameof(Access);
    }

    public static class Attendance
    {
        private const string Prefix = GroupName + "." + nameof(Attendance);

        public const string Access = Prefix + "." + nameof(Access);
    }

    public static (string Name, string DisplayName)[] GetPermissions()
    {
        var rootType = typeof(AcademicPermissions);
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
