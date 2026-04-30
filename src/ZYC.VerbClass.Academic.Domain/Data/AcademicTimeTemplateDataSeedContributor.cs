using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;

namespace ZYC.VerbClass.Academic.Domain.Data;

internal class AcademicTimeTemplateDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public static readonly AcademicTimeTemplateSeedItem[] SeedItems =
    [
        new(
            "DAY-STD",
            "Daytime Standard",
            [
                CreateTermPeriodDefinition(1, "1限", "08:50", "10:20"),
                CreateTermPeriodDefinition(2, "2限", "10:30", "12:00"),
                CreateTermPeriodDefinition(3, "3限", "13:00", "14:30"),
                CreateTermPeriodDefinition(4, "4限", "14:40", "16:10"),
                CreateTermPeriodDefinition(5, "5限", "16:20", "17:50"),
                CreateTermPeriodDefinition(6, "6限", "18:00", "19:30")
            ]),
        new(
            "DAY-LAB",
            "Lab Day",
            [
                CreateTermPeriodDefinition(1, "1限", "09:00", "10:45"),
                CreateTermPeriodDefinition(2, "2限", "11:00", "12:45"),
                CreateTermPeriodDefinition(3, "3限", "13:30", "15:15"),
                CreateTermPeriodDefinition(4, "4限", "15:30", "17:15")
            ]),
        new(
            "EVENING",
            "Evening Program",
            [
                CreateTermPeriodDefinition(1, "1限", "17:50", "19:20"),
                CreateTermPeriodDefinition(2, "2限", "19:30", "21:00"),
                CreateTermPeriodDefinition(3, "3限", "21:10", "22:40")
            ])
    ];

    private readonly AcademicTimeTemplateManager _academicTimeTemplateManager;
    private readonly IAcademicTimeTemplateRepository _academicTimeTemplateRepository;

    private readonly ICurrentTenant _currentTenant;

    public AcademicTimeTemplateDataSeedContributor(
        ICurrentTenant currentTenant,
        AcademicTimeTemplateManager academicTimeTemplateManager,
        IAcademicTimeTemplateRepository academicTimeTemplateRepository)
    {
        _currentTenant = currentTenant;
        _academicTimeTemplateManager = academicTimeTemplateManager;
        _academicTimeTemplateRepository = academicTimeTemplateRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId == null)
        {
            return;
        }

        await EnsureDefaultTemplatesAsync(context.TenantId.Value);
    }

    private static TermPeriodDefinition CreateTermPeriodDefinition(
        int periodNo,
        string label,
        string startTime,
        string endTime)
    {
        return TermPeriodDefinition.Create(periodNo, label, TimeOnly.Parse(startTime), TimeOnly.Parse(endTime));
    }

    internal async Task EnsureDefaultTemplatesAsync(Guid tenantId)
    {
        using (_currentTenant.Change(tenantId))
        {
            foreach (var seedItem in SeedItems)
            {
                var existingTemplate = await _academicTimeTemplateRepository.FindByCodeAsync(seedItem.Code);
                if (existingTemplate is not null)
                {
                    continue;
                }

                var template = await _academicTimeTemplateManager.CreateAsync(
                    seedItem.Code,
                    seedItem.Name,
                    seedItem.Periods.Select(period => TimeTemplatePeriodDefinition.Create(
                        period.PeriodNo,
                        period.Label,
                        period.StartTime,
                        period.EndTime))
                );

                await _academicTimeTemplateRepository.InsertAsync(template, true);
            }
        }
    }

    public record AcademicTimeTemplateSeedItem(
        string Code,
        string Name,
        TermPeriodDefinition[] Periods
    );
}