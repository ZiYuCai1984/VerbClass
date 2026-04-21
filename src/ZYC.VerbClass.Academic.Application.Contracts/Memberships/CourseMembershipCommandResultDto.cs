using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.Memberships;

public class CourseMembershipCommandResultDto
{
    public Guid Id { get; set; }

    public Guid CourseOfferingId { get; set; }

    public Guid UserId { get; set; }

    public CourseMembershipRole Role { get; set; }

    public CourseMembershipStatus Status { get; set; }
}
