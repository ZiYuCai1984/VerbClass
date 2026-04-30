using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;

public class CourseOfferingParticipantListItemDto
{
    public Guid Id { get; set; }

    public Guid CourseOfferingId { get; set; }

    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public CourseOfferingParticipantRole Role { get; set; }
}
