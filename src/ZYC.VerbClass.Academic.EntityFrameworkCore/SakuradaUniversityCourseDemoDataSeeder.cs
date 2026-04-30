using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.Data;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

[ExposeServices(typeof(ISakuradaUniversityTenantDemoDataSeeder))]
internal class SakuradaUniversityCourseDemoDataSeeder : ISakuradaUniversityTenantDemoDataSeeder,
    ITransientDependency
{
    private const int SeedAcademicYear = 2026;
    private const string SeedAcademicTermCode = "2026-first-semester";
    private const string SeedAcademicTermName = "前期";

    private static readonly TermPeriodDefinition[] SeedTermPeriods =
    [
        CreateTermPeriodDefinition(1, "1限", "08:50", "10:20"),
        CreateTermPeriodDefinition(2, "2限", "10:30", "12:00"),
        CreateTermPeriodDefinition(3, "3限", "13:00", "14:30"),
        CreateTermPeriodDefinition(4, "4限", "14:40", "16:10"),
        CreateTermPeriodDefinition(5, "5限", "16:20", "17:50"),
        CreateTermPeriodDefinition(6, "6限", "18:00", "19:30")
    ];

    private static readonly CourseDefinitionSeedItem[] RequiredCourseDefinitionSeedItems =
    [
        new("52220", "電気回路基礎2"),
        new("21140", "プログラミング基礎"),
        new("22310", "データベース概論"),
        new("33170", "ミクロ経済学"),
        new("54010", "教育心理学")
    ];

    private static readonly CourseOfferingSeedItem[] CourseOfferingSeedItems =
    [
        new(
            "2026-52220-A",
            "52220",
            [
                new(AcademicWeekday.Monday, 1),
                new(AcademicWeekday.Wednesday, 2)
            ]),
        new(
            "2026-21140-A",
            "21140",
            [
                new(AcademicWeekday.Tuesday, 2),
                new(AcademicWeekday.Friday, 3)
            ]),
        new(
            "2026-22310-A",
            "22310",
            [
                new(AcademicWeekday.Wednesday, 3)
            ]),
        new(
            "2026-33170-A",
            "33170",
            [
                new(AcademicWeekday.Thursday, 2)
            ]),
        new(
            "2026-54010-A",
            "54010",
            [
                new(AcademicWeekday.Friday, 4)
            ])
    ];

    private static readonly CourseOfferingParticipantSeedItem[] ParticipantSeedItems =
    [
        new("2026-52220-A", "faculty.takumi.takahashi.01", CourseOfferingParticipantRole.Teacher),
        new("2026-52220-A", "staff.yuko.takahashi.01", CourseOfferingParticipantRole.Assistant),
        new("2026-52220-A", "yui.hara", CourseOfferingParticipantRole.Student),
        new("2026-52220-A", "kento.nishimura", CourseOfferingParticipantRole.Student),
        new("2026-52220-A", "noa.sugiyama", CourseOfferingParticipantRole.Student),

        new("2026-21140-A", "faculty.ayaka.takahashi.02", CourseOfferingParticipantRole.Teacher),
        new("2026-21140-A", "staff.shinichi.takahashi.02", CourseOfferingParticipantRole.Assistant),
        new("2026-21140-A", "yuina.ito", CourseOfferingParticipantRole.Student),
        new("2026-21140-A", "rin.ishii", CourseOfferingParticipantRole.Student),
        new("2026-21140-A", "shota.takahashi.01", CourseOfferingParticipantRole.Student),

        new("2026-22310-A", "faculty.kenji.takahashi.03", CourseOfferingParticipantRole.Teacher),
        new("2026-22310-A", "staff.mai.takahashi.03", CourseOfferingParticipantRole.Assistant),
        new("2026-22310-A", "sora.nagase", CourseOfferingParticipantRole.Student),
        new("2026-22310-A", "anna.petrova", CourseOfferingParticipantRole.Student),
        new("2026-22310-A", "yamato.takahashi.02", CourseOfferingParticipantRole.Student),

        new("2026-33170-A", "faculty.mari.takahashi.04", CourseOfferingParticipantRole.Teacher),
        new("2026-33170-A", "staff.kazuya.takahashi.04", CourseOfferingParticipantRole.Assistant),
        new("2026-33170-A", "ren.watanabe", CourseOfferingParticipantRole.Student),
        new("2026-33170-A", "mao.kanda", CourseOfferingParticipantRole.Student),
        new("2026-33170-A", "misaki.takahashi.04", CourseOfferingParticipantRole.Student),

        new("2026-54010-A", "faculty.yusuke.takahashi.05", CourseOfferingParticipantRole.Teacher),
        new("2026-54010-A", "staff.nana.takahashi.05", CourseOfferingParticipantRole.Assistant),
        new("2026-54010-A", "haruto.sasaki", CourseOfferingParticipantRole.Student),
        new("2026-54010-A", "yuka.mizuno", CourseOfferingParticipantRole.Student),
        new("2026-54010-A", "arata.okada", CourseOfferingParticipantRole.Student),
        new("2026-54010-A", "mina.lee", CourseOfferingParticipantRole.Observer)
    ];

    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantRepository _tenantRepository;
    private readonly SakuradaUniversityIdentityDemoDataSeeder _identityDemoDataSeeder;
    private readonly IdentityUserManager _userManager;
    private readonly IAcademicTermRepository _academicTermRepository;
    private readonly AcademicTermManager _academicTermManager;
    private readonly ICourseDefinitionRepository _courseDefinitionRepository;
    private readonly CourseDefinitionManager _courseDefinitionManager;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly CourseOfferingManager _courseOfferingManager;
    private readonly ICourseOfferingParticipantRepository _participantRepository;
    private readonly CourseOfferingParticipantManager _participantManager;

    public SakuradaUniversityCourseDemoDataSeeder(
        ICurrentTenant currentTenant,
        ITenantRepository tenantRepository,
        SakuradaUniversityIdentityDemoDataSeeder identityDemoDataSeeder,
        IdentityUserManager userManager,
        IAcademicTermRepository academicTermRepository,
        AcademicTermManager academicTermManager,
        ICourseDefinitionRepository courseDefinitionRepository,
        CourseDefinitionManager courseDefinitionManager,
        ICourseOfferingRepository courseOfferingRepository,
        CourseOfferingManager courseOfferingManager,
        ICourseOfferingParticipantRepository participantRepository,
        CourseOfferingParticipantManager participantManager)
    {
        _currentTenant = currentTenant;
        _tenantRepository = tenantRepository;
        _identityDemoDataSeeder = identityDemoDataSeeder;
        _userManager = userManager;
        _academicTermRepository = academicTermRepository;
        _academicTermManager = academicTermManager;
        _courseDefinitionRepository = courseDefinitionRepository;
        _courseDefinitionManager = courseDefinitionManager;
        _courseOfferingRepository = courseOfferingRepository;
        _courseOfferingManager = courseOfferingManager;
        _participantRepository = participantRepository;
        _participantManager = participantManager;
    }

    public int Order => 30;

    public async Task SeedAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.FindAsync(tenantId)
                     ?? throw new AbpException(
                         $"Tenant '{tenantId}' was not found for Sakura demo course seeding."
                     );

        if (!string.Equals(tenant.Name, VerbClassConsts.SakuradaUniversityDemoTenantName, StringComparison.Ordinal))
        {
            throw new AbpException(
                $"Tenant '{tenant.Name}' is not the Sakura University demo tenant."
            );
        }

        var usersByUserName = await _identityDemoDataSeeder.EnsureDemoAccountsAsync(tenantId);

        using (_currentTenant.Change(tenantId))
        {
            var academicTerm = await EnsureAcademicTermAsync();
            var courseDefinitionsByCode = await EnsureCourseDefinitionsAsync();
            var courseOfferingsByCode = await EnsureCourseOfferingsAsync(academicTerm, courseDefinitionsByCode);

            await EnsureParticipantsAsync(usersByUserName, courseOfferingsByCode);
        }
    }

    private async Task<AcademicTerm> EnsureAcademicTermAsync()
    {
        var academicTerm = await _academicTermRepository.FindByYearAndCodeAsync(
            SeedAcademicYear,
            SeedAcademicTermCode
        );

        if (academicTerm != null)
        {
            return academicTerm;
        }

        academicTerm = await _academicTermManager.CreateAsync(
            SeedAcademicYear,
            SeedAcademicTermCode,
            SeedAcademicTermName,
            new DateTime(2026, 4, 1),
            new DateTime(2026, 10, 1),
            SeedTermPeriods
        );

        return await _academicTermRepository.InsertAsync(academicTerm, true);
    }

    private async Task<Dictionary<string, CourseDefinition>> EnsureCourseDefinitionsAsync()
    {
        var courseDefinitionsByCode = new Dictionary<string, CourseDefinition>(StringComparer.Ordinal);

        foreach (var seedItem in RequiredCourseDefinitionSeedItems)
        {
            var courseDefinition = await _courseDefinitionRepository.FindByCodeAsync(seedItem.Code);
            if (courseDefinition is null)
            {
                courseDefinition = await _courseDefinitionManager.CreateAsync(seedItem.Code, seedItem.Name);
                courseDefinition = await _courseDefinitionRepository.InsertAsync(courseDefinition, true);
            }

            courseDefinitionsByCode.Add(seedItem.Code, courseDefinition);
        }

        return courseDefinitionsByCode;
    }

    private async Task<Dictionary<string, CourseOffering>> EnsureCourseOfferingsAsync(
        AcademicTerm academicTerm,
        IReadOnlyDictionary<string, CourseDefinition> courseDefinitionsByCode)
    {
        var courseOfferingsByCode = (await _courseOfferingRepository.GetListByTermAsync(academicTerm.Id))
            .ToDictionary(x => x.OfferingCode, StringComparer.Ordinal);

        foreach (var seedItem in CourseOfferingSeedItems)
        {
            var courseDefinition = GetRequiredCourseDefinition(courseDefinitionsByCode, seedItem.CourseDefinitionCode);
            if (courseOfferingsByCode.TryGetValue(seedItem.OfferingCode, out var existingOffering))
            {
                if (existingOffering.CourseDefinitionId != courseDefinition.Id)
                {
                    throw new AbpException(
                        $"Course offering '{seedItem.OfferingCode}' already exists with a different course definition."
                    );
                }

                continue;
            }

            var courseOffering = await _courseOfferingManager.CreateAsync(
                academicTerm,
                courseDefinition,
                seedItem.OfferingCode,
                seedItem.ScheduleSlots.Select(x => CourseOfferingScheduleSlot.Create(x.Weekday, x.PeriodNo))
            );

            courseOffering = await _courseOfferingRepository.InsertAsync(courseOffering, true);
            courseOfferingsByCode.Add(seedItem.OfferingCode, courseOffering);
        }

        return courseOfferingsByCode;
    }

    private async Task EnsureParticipantsAsync(
        IReadOnlyDictionary<string, IdentityUser> usersByUserName,
        IReadOnlyDictionary<string, CourseOffering> courseOfferingsByCode)
    {
        var existingParticipantsByAssignment = (await _participantRepository.GetListByOfferingsAsync(
                CourseOfferingSeedItems
                    .Select(x => GetRequiredCourseOffering(courseOfferingsByCode, x.OfferingCode).Id)
                    .ToArray()
            ))
            .ToDictionary(x => (x.CourseOfferingId, x.UserId));

        foreach (var seedItem in ParticipantSeedItems)
        {
            var courseOffering = GetRequiredCourseOffering(courseOfferingsByCode, seedItem.OfferingCode);
            var user = GetRequiredUser(usersByUserName, seedItem.UserName);

            await EnsureUserQualifiedForRoleAsync(user, seedItem.Role);

            if (existingParticipantsByAssignment.TryGetValue((courseOffering.Id, user.Id), out var existingParticipant))
            {
                if (existingParticipant.Role != seedItem.Role)
                {
                    await _participantManager.ChangeRoleAsync(existingParticipant, seedItem.Role);
                    await _participantRepository.UpdateAsync(existingParticipant, true);
                }

                continue;
            }

            var participant = await _participantManager.CreateAsync(
                courseOffering,
                user.Id,
                seedItem.Role
            );

            participant = await _participantRepository.InsertAsync(participant, true);
            existingParticipantsByAssignment.Add((participant.CourseOfferingId, participant.UserId), participant);
        }
    }

    private async Task EnsureUserQualifiedForRoleAsync(
        IdentityUser user,
        CourseOfferingParticipantRole role)
    {
        var requiredRoleName = role switch
        {
            CourseOfferingParticipantRole.Teacher => VerbClassRoles.Instructor,
            CourseOfferingParticipantRole.Assistant => VerbClassRoles.Assistant,
            CourseOfferingParticipantRole.Student => VerbClassRoles.Student,
            CourseOfferingParticipantRole.Observer => null,
            _ => throw new BusinessException(CourseOfferingParticipantErrorCodes.InvalidRole)
                .WithData(nameof(CourseOfferingParticipant.Role), role)
        };

        if (requiredRoleName is null)
        {
            return;
        }

        if (!await _userManager.IsInRoleAsync(user, requiredRoleName))
        {
            throw new AbpException(
                $"Seed user '{user.UserName}' must have role '{requiredRoleName}' to be assigned as '{role}'."
            );
        }
    }

    private static CourseDefinition GetRequiredCourseDefinition(
        IReadOnlyDictionary<string, CourseDefinition> courseDefinitionsByCode,
        string code)
    {
        return courseDefinitionsByCode.TryGetValue(code, out var courseDefinition)
            ? courseDefinition
            : throw new AbpException($"Course definition '{code}' was not seeded before offering seeding.");
    }

    private static CourseOffering GetRequiredCourseOffering(
        IReadOnlyDictionary<string, CourseOffering> courseOfferingsByCode,
        string offeringCode)
    {
        return courseOfferingsByCode.TryGetValue(offeringCode, out var courseOffering)
            ? courseOffering
            : throw new AbpException($"Course offering '{offeringCode}' was not seeded before participant seeding.");
    }

    private static IdentityUser GetRequiredUser(
        IReadOnlyDictionary<string, IdentityUser> usersByUserName,
        string userName)
    {
        return usersByUserName.TryGetValue(userName, out var user)
            ? user
            : throw new AbpException($"Seed user '{userName}' was not created before participant seeding.");
    }

    private static TermPeriodDefinition CreateTermPeriodDefinition(
        int periodNo,
        string label,
        string startTime,
        string endTime)
    {
        return TermPeriodDefinition.Create(periodNo, label, TimeOnly.Parse(startTime), TimeOnly.Parse(endTime));
    }

    private record CourseDefinitionSeedItem(string Code, string Name);

    private record CourseOfferingSeedItem(
        string OfferingCode,
        string CourseDefinitionCode,
        CourseOfferingScheduleSlotSeedItem[] ScheduleSlots);

    private record CourseOfferingScheduleSlotSeedItem(AcademicWeekday Weekday, int PeriodNo);

    private record CourseOfferingParticipantSeedItem(
        string OfferingCode,
        string UserName,
        CourseOfferingParticipantRole Role);
}
