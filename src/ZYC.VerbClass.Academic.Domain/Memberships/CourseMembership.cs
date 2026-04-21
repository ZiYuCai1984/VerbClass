using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.Memberships;

public class CourseMembership : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    protected CourseMembership()
    {
    }

    public CourseMembership(
        Guid id,
        Guid tenantId,
        Guid courseOfferingId,
        Guid userId,
        CourseMembershipRole role) : base(id)
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
        SetStatus(CourseMembershipStatus.Active);
    }

    public Guid? TenantId { get; protected set; }

    public Guid CourseOfferingId { get; private set; }

    public Guid UserId { get; private set; }

    public CourseMembershipRole Role { get; private set; }

    public CourseMembershipStatus Status { get; private set; }

    public void JoinAsStudent()
    {
        ChangeRole(CourseMembershipRole.Student);
        SetStatus(CourseMembershipStatus.Active);
    }

    public void Drop()
    {
        if (Status != CourseMembershipStatus.Active)
        {
            throw new BusinessException(AcademicErrorCodes.InvalidMembershipStatus)
                .WithData("Status", Status);
        }

        SetStatus(CourseMembershipStatus.Dropped);
    }

    public void ChangeRole(CourseMembershipRole role)
    {
        if (!Enum.IsDefined(typeof(CourseMembershipRole), role))
        {
            throw new BusinessException(AcademicErrorCodes.InvalidMembershipRole)
                .WithData("Role", role);
        }

        Role = role;
    }

    public bool CanRejoinAsStudent()
    {
        return Status == CourseMembershipStatus.Dropped;
    }

    private void SetStatus(CourseMembershipStatus status)
    {
        if (!Enum.IsDefined(typeof(CourseMembershipStatus), status))
        {
            throw new BusinessException(AcademicErrorCodes.InvalidMembershipStatus)
                .WithData("Status", status);
        }

        Status = status;
    }
}
