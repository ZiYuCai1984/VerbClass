using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class CourseOfferingManager : DomainService
{
    private readonly IRepository<CourseDefinition, Guid> _courseDefinitionRepository;
    private readonly ICurrentTenant _currentTenant;

    public CourseOfferingManager(
        IRepository<CourseDefinition, Guid> courseDefinitionRepository,
        ICurrentTenant currentTenant)
    {
        _courseDefinitionRepository = courseDefinitionRepository;
        _currentTenant = currentTenant;
    }

    public virtual async Task<CourseOffering> CreateAsync(
        Guid courseDefinitionId,
        AcademicTerm term,
        DayOfWeek? scheduleDayOfWeek,
        TimeSpan? scheduleStartTime,
        TimeSpan? scheduleEndTime,
        string? scheduleLocation,
        EnrollmentPolicy enrollmentPolicy,
        CancellationToken cancellationToken = default)
    {
        if (courseDefinitionId == Guid.Empty)
        {
            throw new BusinessException(AcademicErrorCodes.CourseDefinitionNotFound);
        }

        var courseDefinition = await _courseDefinitionRepository.FindAsync(
            courseDefinitionId,
            cancellationToken: cancellationToken
        );

        if (courseDefinition is null)
        {
            throw new BusinessException(AcademicErrorCodes.CourseDefinitionNotFound)
                .WithData("CourseDefinitionId", courseDefinitionId);
        }

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("CourseOffering must be created within a tenant context.");

        return new CourseOffering(
            GuidGenerator.Create(),
            tenantId,
            courseDefinitionId,
            term,
            scheduleDayOfWeek,
            scheduleStartTime,
            scheduleEndTime,
            scheduleLocation,
            enrollmentPolicy
        );
    }
}
