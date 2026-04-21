using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Academic.Application.Contracts.Memberships;

public class JoinCourseMembershipInput
{
    [Required]
    public Guid CourseOfferingId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}
