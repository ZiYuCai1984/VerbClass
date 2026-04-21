using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.Departments;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.Domain.Data;

internal class VerbClassDemoDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<DepartmentAssignment, Guid> _departmentAssignmentRepository;
    private readonly VerbClassDepartmentDataSeedContributor _departmentDataSeedContributor;
    private readonly IGuidGenerator _guidGenerator;
    private readonly VerbClassIdentityDataSeedContributor _identityDataSeedContributor;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRepository<UserProfile, Guid> _userProfileRepository;

    public VerbClassDemoDataSeedContributor(
        ICurrentTenant currentTenant,
        IRepository<DepartmentAssignment, Guid> departmentAssignmentRepository,
        VerbClassDepartmentDataSeedContributor departmentDataSeedContributor,
        IGuidGenerator guidGenerator,
        VerbClassIdentityDataSeedContributor identityDataSeedContributor,
        ITenantRepository tenantRepository,
        IRepository<UserProfile, Guid> userProfileRepository)
    {
        _currentTenant = currentTenant;
        _departmentAssignmentRepository = departmentAssignmentRepository;
        _departmentDataSeedContributor = departmentDataSeedContributor;
        _guidGenerator = guidGenerator;
        _identityDataSeedContributor = identityDataSeedContributor;
        _tenantRepository = tenantRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId == null)
        {
            return;
        }

        var tenant = await _tenantRepository.FindAsync(context.TenantId.Value);
        if (!string.Equals(tenant?.Name, SakuradaUniversitySeedData.InitialTenantName, StringComparison.Ordinal))
        {
            return;
        }

        var departmentsByCode =
            await _departmentDataSeedContributor.EnsureDefaultDepartmentsAsync(context.TenantId.Value);
        var usersByUserName = await _identityDataSeedContributor.EnsureDemoAccountsAsync(context.TenantId.Value);

        using (_currentTenant.Change(context.TenantId.Value))
        {
            await SeedUsersAsync(usersByUserName, departmentsByCode);
        }
    }

    private async Task SeedUsersAsync(
        IReadOnlyDictionary<string, IdentityUser> usersByUserName,
        IReadOnlyDictionary<string, Department> departmentsByCode)
    {
        foreach (var seedItem in SakuradaUniversitySeedData.UserSeedItems)
        {
            if (!departmentsByCode.TryGetValue(seedItem.DepartmentCode, out var department))
            {
                throw new AbpException(
                    $"Department '{seedItem.DepartmentCode}' was not found for user '{seedItem.UserName}'."
                );
            }

            if (!usersByUserName.TryGetValue(seedItem.UserName, out var user))
            {
                throw new AbpException($"User '{seedItem.UserName}' was not created before profile seeding.");
            }

            await EnsureUserProfileAsync(user.Id, seedItem);
            await EnsureDepartmentAssignmentAsync(user.Id, department.Id, seedItem.DepartmentEffectiveFrom);
        }
    }

    private async Task EnsureUserProfileAsync(Guid userId, UniversityUserSeedItem seedItem)
    {
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);

        if (profile is null)
        {
            profile = new UserProfile(
                _guidGenerator.Create(),
                _currentTenant.GetId(),
                userId,
                new PersonNameInfo(
                    seedItem.SurnameKanji,
                    seedItem.NameKanji,
                    seedItem.SurnameKana,
                    seedItem.NameKana,
                    seedItem.SurnameRomanized,
                    seedItem.NameRomanized
                ),
                seedItem.BirthDate,
                seedItem.Gender,
                seedItem.BloodType,
                seedItem.Nationality,
                new AddressInfo(
                    seedItem.Country,
                    seedItem.Prefecture,
                    seedItem.City,
                    seedItem.Street,
                    seedItem.PostalCode
                ),
                enrollmentYear: seedItem.EnrollmentYear
            );

            await _userProfileRepository.InsertAsync(profile, true);
            return;
        }

        profile.ChangeName(
            seedItem.SurnameKanji,
            seedItem.NameKanji,
            seedItem.SurnameKana,
            seedItem.NameKana,
            seedItem.SurnameRomanized,
            seedItem.NameRomanized
        );
        profile.ChangeBirthDate(seedItem.BirthDate);
        profile.ChangeGender(seedItem.Gender);
        profile.ChangeBloodType(seedItem.BloodType);
        profile.ChangeNationality(seedItem.Nationality);
        profile.ChangeAddress(
            seedItem.Country,
            seedItem.Prefecture,
            seedItem.City,
            seedItem.Street,
            seedItem.PostalCode
        );
        profile.ChangeEnrollmentYear(seedItem.EnrollmentYear);

        await _userProfileRepository.UpdateAsync(profile, true);
    }

    private async Task EnsureDepartmentAssignmentAsync(
        Guid userId,
        Guid departmentId,
        DateTime effectiveFrom)
    {
        var normalizedEffectiveFrom = effectiveFrom.Date;
        var assignments = await _departmentAssignmentRepository.GetListAsync(x => x.UserId == userId);
        var primaryAssignment = assignments.FirstOrDefault(x => x.IsPrimary) ?? assignments.FirstOrDefault();

        if (primaryAssignment is null)
        {
            primaryAssignment = new DepartmentAssignment(
                _guidGenerator.Create(),
                _currentTenant.GetId(),
                userId,
                departmentId,
                true,
                normalizedEffectiveFrom
            );

            await _departmentAssignmentRepository.InsertAsync(primaryAssignment, true);
            return;
        }

        var changed = false;

        if (primaryAssignment.DepartmentId != departmentId)
        {
            primaryAssignment.ChangeDepartment(departmentId);
            changed = true;
        }

        if (!primaryAssignment.IsPrimary)
        {
            primaryAssignment.SetPrimary(true);
            changed = true;
        }

        if (primaryAssignment.EffectiveFrom.Date != normalizedEffectiveFrom ||
            primaryAssignment.EffectiveTo.HasValue)
        {
            primaryAssignment.ChangePeriod(normalizedEffectiveFrom, null);
            changed = true;
        }

        if (changed)
        {
            await _departmentAssignmentRepository.UpdateAsync(primaryAssignment, true);
        }

        foreach (var assignment in assignments.Where(x => x.Id != primaryAssignment.Id && x.IsPrimary))
        {
            assignment.SetPrimary(false);
            await _departmentAssignmentRepository.UpdateAsync(assignment, true);
        }
    }
}
