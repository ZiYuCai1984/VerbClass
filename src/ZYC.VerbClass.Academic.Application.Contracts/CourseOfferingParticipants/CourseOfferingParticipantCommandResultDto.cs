using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;

public class CourseOfferingParticipantCommandResultDto
{
    public Guid Id { get; set; }

    public Guid CourseOfferingId { get; set; }

    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public CourseOfferingParticipantRole Role { get; set; }
}
