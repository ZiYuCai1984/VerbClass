using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;

public class CourseOfferingParticipantManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICourseOfferingParticipantRepository _participantRepository;

    public CourseOfferingParticipantManager(
        ICurrentTenant currentTenant,
        ICourseOfferingParticipantRepository participantRepository)
    {
        _currentTenant = currentTenant;
        _participantRepository = participantRepository;
    }

    public virtual async Task<CourseOfferingParticipant> CreateAsync(
        CourseOffering courseOffering,
        Guid userId,
        CourseOfferingParticipantRole role,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(courseOffering, nameof(courseOffering));

        if (userId == Guid.Empty)
        {
            throw new AbpException("UserId can not be empty.");
        }

        var normalizedRole = CourseOfferingParticipant.NormalizeRole(role);

        await ValidateUserAssignmentAsync(
            courseOffering.Id,
            userId,
            null,
            cancellationToken
        );

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("Course offering participant must be created within a tenant context.");

        return new CourseOfferingParticipant(
            GuidGenerator.Create(),
            tenantId,
            courseOffering.Id,
            userId,
            normalizedRole
        );
    }

    public virtual async Task ChangeRoleAsync(
        CourseOfferingParticipant participant,
        CourseOfferingParticipantRole role,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(participant, nameof(participant));

        CourseOfferingParticipant.NormalizeRole(role);

        await ValidateUserAssignmentAsync(
            participant.CourseOfferingId,
            participant.UserId,
            participant.Id,
            cancellationToken
        );

        participant.ChangeRole(role);
    }

    protected virtual async Task ValidateUserAssignmentAsync(
        Guid courseOfferingId,
        Guid userId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _participantRepository.IsUserAssignedAsync(
            courseOfferingId,
            userId,
            excludeId,
            cancellationToken
        );

        if (exists)
        {
            throw new BusinessException(CourseOfferingParticipantErrorCodes.UserAlreadyAssigned)
                .WithData(nameof(CourseOfferingParticipant.CourseOfferingId), courseOfferingId)
                .WithData(nameof(CourseOfferingParticipant.UserId), userId);
        }
    }
}
