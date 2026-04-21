using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Domain.Departments;

namespace ZYC.VerbClass.Domain.Data;

internal class VerbClassDepartmentDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private static readonly DepartmentSeedItem[] DepartmentSeedItems =
    [
        new("presidents-office", "学長室", null, 10),
        new("university-administration", "大学事務局", null, 20),
        new("general-affairs", "総務課", "university-administration", 10),
        new("academic-affairs", "教務課", "university-administration", 20),
        new("student-support", "学生支援課", "university-administration", 30),
        new("admissions", "入試課", "university-administration", 40),
        new("international-exchange", "国際交流課", "university-administration", 50),
        new("information-systems", "情報システム課", "university-administration", 60),
        new("university-library", "図書館", null, 30),
        new("career-center", "キャリアセンター", null, 40),
        new("health-center", "保健管理センター", null, 50),
        new("faculty-engineering", "工学部", null, 60),
        new("department-information-engineering", "情報工学科", "faculty-engineering", 10),
        new("department-electrical-electronics-engineering", "電気電子工学科", "faculty-engineering", 20),
        new("department-mechanical-engineering", "機械工学科", "faculty-engineering", 30),
        new("faculty-science", "理学部", null, 70),
        new("department-mathematics", "数学科", "faculty-science", 10),
        new("department-physics", "物理学科", "faculty-science", 20),
        new("faculty-economics", "経済学部", null, 80),
        new("department-economics", "経済学科", "faculty-economics", 10),
        new("department-business-administration", "経営学科", "faculty-economics", 20),
        new("graduate-school", "大学院", null, 90),
        new("graduate-school-engineering", "工学研究科", "graduate-school", 10),
        new("graduate-school-science", "理学研究科", "graduate-school", 20),
        new("affiliated-institutions", "附属機関", null, 100),
        new("affiliated-library", "附属図書館", "affiliated-institutions", 10),
        new("affiliated-high-school", "附属高等学校", "affiliated-institutions", 20)
    ];

    private readonly ICurrentTenant _currentTenant;
    private readonly DepartmentManager _departmentManager;
    private readonly IDepartmentRepository _departmentRepository;

    public VerbClassDepartmentDataSeedContributor(
        ICurrentTenant currentTenant,
        DepartmentManager departmentManager,
        IDepartmentRepository departmentRepository)
    {
        _currentTenant = currentTenant;
        _departmentManager = departmentManager;
        _departmentRepository = departmentRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId == null)
        {
            return;
        }

        await EnsureDefaultDepartmentsAsync(context.TenantId.Value);
    }

    internal async Task<Dictionary<string, Department>> EnsureDefaultDepartmentsAsync(Guid tenantId)
    {
        using (_currentTenant.Change(tenantId))
        {
            var departmentsByCode = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);

            foreach (var seedItem in DepartmentSeedItems)
            {
                var parent = await FindDepartmentAsync(seedItem.ParentCode, departmentsByCode);
                if (seedItem.ParentCode is not null && parent is null)
                {
                    throw new AbpException(
                        $"Department seed parent '{seedItem.ParentCode}' was not found for '{seedItem.Code}'."
                    );
                }

                var department = await FindDepartmentAsync(seedItem.Code, departmentsByCode);

                if (department is null)
                {
                    department = await _departmentManager.CreateAsync(
                        seedItem.Code,
                        seedItem.Name,
                        parent?.Id,
                        sort: seedItem.Sort
                    );

                    await _departmentRepository.InsertAsync(department, true);
                    departmentsByCode[seedItem.Code] = department;
                    continue;
                }

                departmentsByCode[seedItem.Code] = department;
            }

            return departmentsByCode;
        }
    }

    private async Task<Department?> FindDepartmentAsync(
        string? code,
        Dictionary<string, Department> departmentsByCode)
    {
        if (code is null)
        {
            return null;
        }

        if (departmentsByCode.TryGetValue(code, out var department))
        {
            return department;
        }

        department = await _departmentRepository.FindByCodeAsync(code);
        if (department is not null)
        {
            departmentsByCode[code] = department;
        }

        return department;
    }

    private static string BuildPath(Department? parent, Guid departmentId)
    {
        return parent is null
            ? $"/{departmentId}/"
            : $"{parent.Path}{departmentId}/";
    }

    private record DepartmentSeedItem(
        string Code,
        string Name,
        string? ParentCode,
        int Sort
    );
}
