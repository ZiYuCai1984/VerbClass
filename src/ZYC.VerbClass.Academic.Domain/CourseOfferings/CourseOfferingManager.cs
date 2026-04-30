using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class CourseOfferingManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICourseOfferingRepository _courseOfferingRepository;

    public CourseOfferingManager(
        ICurrentTenant currentTenant,
        ICourseOfferingRepository courseOfferingRepository)
    {
        _currentTenant = currentTenant;
        _courseOfferingRepository = courseOfferingRepository;
    }

    public virtual async Task<CourseOffering> CreateAsync(
        AcademicTerm academicTerm,
        CourseDefinition courseDefinition,
        string offeringCode,
        IEnumerable<CourseOfferingScheduleSlot> scheduleSlots,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(academicTerm, nameof(academicTerm));
        Check.NotNull(courseDefinition, nameof(courseDefinition));

        courseDefinition.EnsureActiveForOffering();

        var normalizedOfferingCode = CourseOffering.NormalizeOfferingCode(offeringCode);

        await ValidateOfferingCodeAsync(
            academicTerm.Id,
            normalizedOfferingCode,
            null,
            cancellationToken
        );

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("Course offering must be created within a tenant context.");

        return new CourseOffering(
            GuidGenerator.Create(),
            tenantId,
            academicTerm.Id,
            courseDefinition.Id,
            normalizedOfferingCode,
            courseDefinition.Code,
            courseDefinition.Name,
            scheduleSlots,
            academicTerm.Periods.Select(x => x.PeriodNo)
        );
    }

    public virtual async Task ChangeOfferingCodeAsync(
        CourseOffering courseOffering,
        string offeringCode,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(courseOffering, nameof(courseOffering));

        var normalizedOfferingCode = CourseOffering.NormalizeOfferingCode(offeringCode);

        await ValidateOfferingCodeAsync(
            courseOffering.AcademicTermId,
            normalizedOfferingCode,
            courseOffering.Id,
            cancellationToken
        );

        courseOffering.ChangeOfferingCode(normalizedOfferingCode);
    }

    protected virtual async Task ValidateOfferingCodeAsync(
        Guid academicTermId,
        string offeringCode,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _courseOfferingRepository.IsOfferingCodeExistsAsync(
            academicTermId,
            offeringCode,
            excludeId,
            cancellationToken
        );

        if (exists)
        {
            throw new BusinessException(CourseOfferingErrorCodes.OfferingCodeAlreadyExists)
                .WithData(nameof(CourseOffering.AcademicTermId), academicTermId)
                .WithData(nameof(CourseOffering.OfferingCode), offeringCode);
        }
    }
}
