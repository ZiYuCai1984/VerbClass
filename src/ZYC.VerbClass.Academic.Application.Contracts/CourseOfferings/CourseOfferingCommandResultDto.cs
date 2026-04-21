using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public class CourseOfferingCommandResultDto
{
    public Guid Id { get; set; }

    public Guid CourseDefinitionId { get; set; }

    public int AcademicYear { get; set; }

    public string TermName { get; set; } = string.Empty;

    public CourseOfferingStatus Status { get; set; }

    public bool IsLocked { get; set; }
}
