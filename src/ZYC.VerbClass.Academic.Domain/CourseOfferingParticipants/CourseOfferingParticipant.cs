using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;

[Audited]
public class CourseOfferingParticipant : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    protected CourseOfferingParticipant()
    {
    }

    public CourseOfferingParticipant(
        Guid id,
        Guid tenantId,
        Guid courseOfferingId,
        Guid userId,
        CourseOfferingParticipantRole role) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        if (courseOfferingId == Guid.Empty)
        {
            throw new AbpException("CourseOfferingId can not be empty.");
        }

        if (userId == Guid.Empty)
        {
            throw new AbpException("UserId can not be empty.");
        }

        TenantId = tenantId;
        CourseOfferingId = courseOfferingId;
        UserId = userId;
        ChangeRole(role);
    }

    public Guid? TenantId { get; protected set; }

    public Guid CourseOfferingId { get; private set; }

    public Guid UserId { get; private set; }

    public CourseOfferingParticipantRole Role { get; private set; }

    public void ChangeRole(CourseOfferingParticipantRole role)
    {
        Role = NormalizeRole(role);
    }

    internal static CourseOfferingParticipantRole NormalizeRole(CourseOfferingParticipantRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new BusinessException(CourseOfferingParticipantErrorCodes.InvalidRole)
                .WithData(nameof(Role), role);
        }

        return role;
    }
}
