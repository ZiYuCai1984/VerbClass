using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.Memberships;

public class CourseMembershipManager : DomainService
{
    private readonly IRepository<CourseMembership, Guid> _courseMembershipRepository;
    private readonly IRepository<CourseOffering, Guid> _courseOfferingRepository;
    private readonly ICurrentTenant _currentTenant;

    public CourseMembershipManager(
        IRepository<CourseMembership, Guid> courseMembershipRepository,
        IRepository<CourseOffering, Guid> courseOfferingRepository,
        ICurrentTenant currentTenant)
    {
        _courseMembershipRepository = courseMembershipRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _currentTenant = currentTenant;
    }

    public virtual async Task<CourseMembership> JoinStudentAsync(
        Guid courseOfferingId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (courseOfferingId == Guid.Empty)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingNotFound);
        }

        if (userId == Guid.Empty)
        {
            throw new BusinessException(AcademicErrorCodes.MembershipUserNotFound);
        }

        var courseOffering = await _courseOfferingRepository.FindAsync(
            courseOfferingId,
            cancellationToken: cancellationToken
        );

        if (courseOffering is null)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingNotFound)
                .WithData("CourseOfferingId", courseOfferingId);
        }

        courseOffering.EnsureAllowsStudentJoin(Clock.Now);

        var existingMembership = await _courseMembershipRepository.FindAsync(
            x => x.CourseOfferingId == courseOfferingId && x.UserId == userId,
            cancellationToken: cancellationToken
        );

        if (existingMembership is not null)
        {
            if (existingMembership.CanRejoinAsStudent())
            {
                existingMembership.JoinAsStudent();
                return existingMembership;
            }

            throw new BusinessException(AcademicErrorCodes.MembershipAlreadyExists)
                .WithData("CourseOfferingId", courseOfferingId)
                .WithData("UserId", userId);
        }

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("CourseMembership must be created within a tenant context.");

        return new CourseMembership(
            GuidGenerator.Create(),
            tenantId,
            courseOfferingId,
            userId,
            CourseMembershipRole.Student
        );
    }
}
