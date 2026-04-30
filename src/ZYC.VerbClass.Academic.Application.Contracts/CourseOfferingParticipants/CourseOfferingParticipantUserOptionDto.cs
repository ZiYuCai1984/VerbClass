namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;

public class CourseOfferingParticipantUserOptionDto
{
    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string[] RoleNames { get; set; } = [];
}
