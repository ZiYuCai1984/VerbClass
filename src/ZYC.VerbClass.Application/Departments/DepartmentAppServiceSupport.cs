using ZYC.VerbClass.Application.Contracts.Departments;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.Departments;

namespace ZYC.VerbClass.Application.Departments;

internal static class DepartmentAppServiceSupport
{
    public static Department[] OrderDepartments(IReadOnlyList<Department> departments)
    {
        if (departments.Count == 0)
        {
            return [];
        }

        return OrderDepartmentsCore(departments)
            .Select(x => x.Department)
            .ToArray();
    }

    public static DepartmentListItemDto[] BuildListItems(
        IReadOnlyList<Department> departments,
        IReadOnlyDictionary<Guid, int> currentUserCounts)
    {
        if (departments.Count == 0)
        {
            return [];
        }

        var departmentsById = departments.ToDictionary(x => x.Id);
        return OrderDepartmentsCore(departments)
            .Select(x => VerbClassApplicationMappers.ToDepartmentListItemDto(
                x.Department,
                BuildDepartmentDisplayPath(x.Department, departmentsById),
                x.Depth,
                currentUserCounts.GetValueOrDefault(x.Department.Id)))
            .ToArray();
    }

    public static DepartmentAssignment[] SelectCurrentAssignments(
        IEnumerable<DepartmentAssignment> assignments,
        DateTime effectiveDate)
    {
        return assignments
            .Where(x => x.EffectiveFrom.Date <= effectiveDate &&
                        (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= effectiveDate))
            .GroupBy(x => x.DepartmentId)
            .Select(group => group
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveFrom)
                .ThenBy(x => x.Id)
                .First())
            .ToArray();
    }

    public static UserDepartmentDisplayItemDto[] BuildDepartmentDisplayItems(
        IEnumerable<DepartmentAssignment> assignments,
        DateTime effectiveDate,
        IReadOnlyDictionary<Guid, Department> departmentsById)
    {
        return SelectCurrentAssignments(assignments, effectiveDate)
            .Select(assignment => departmentsById.TryGetValue(assignment.DepartmentId, out var department)
                ? VerbClassApplicationMappers.ToUserDepartmentDisplayItemDto(
                    assignment,
                    BuildDepartmentDisplayPath(department, departmentsById))
                : null)
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string? BuildDepartmentSummary(IReadOnlyList<UserDepartmentDisplayItemDto> departments)
    {
        return departments.Count switch
        {
            0 => null,
            1 => departments[0].Path,
            2 => string.Join(", ", departments.Select(x => x.Path)),
            _ => $"{departments[0].Path} (+{departments.Count - 1})"
        };
    }

    public static string BuildDepartmentDisplayPath(
        Department department,
        IReadOnlyDictionary<Guid, Department> departmentsById)
    {
        if (!string.IsNullOrWhiteSpace(department.Path))
        {
            var segments = department.Path
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(segment => Guid.TryParse(segment, out var departmentId) &&
                                   departmentsById.TryGetValue(departmentId, out var pathDepartment)
                    ? pathDepartment.Name
                    : null)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray();

            if (segments.Length > 0)
            {
                return "/" + string.Join('/', segments);
            }
        }

        return "/" + department.Name;
    }

    private static DepartmentHierarchyEntry[] OrderDepartmentsCore(IReadOnlyList<Department> departments)
    {
        var departmentsByParentId = departments.ToLookup(x => x.ParentDepartmentId);
        var orderedDepartments = new List<DepartmentHierarchyEntry>(departments.Count);
        var visited = new HashSet<Guid>();

        void AppendChildren(Guid? parentDepartmentId, int depth)
        {
            foreach (var department in departmentsByParentId[parentDepartmentId]
                         .OrderBy(item => item.Sort)
                         .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (!visited.Add(department.Id))
                {
                    continue;
                }

                orderedDepartments.Add(new DepartmentHierarchyEntry(department, depth));
                AppendChildren(department.Id, depth + 1);
            }
        }

        AppendChildren(null, 0);

        foreach (var department in departments
                     .Where(x => !visited.Contains(x.Id))
                     .OrderBy(x => x.Sort)
                     .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            orderedDepartments.Add(new DepartmentHierarchyEntry(department, 0));
        }

        return orderedDepartments.ToArray();
    }

    private readonly record struct DepartmentHierarchyEntry(Department Department, int Depth);
}
