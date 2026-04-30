using ZYC.VerbClass.Academic.Application.Contracts.Academic;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public class CourseOfferingListItemDto
{
    public Guid Id { get; set; }

    public Guid AcademicTermId { get; set; }

    public Guid CourseDefinitionId { get; set; }

    public string OfferingCode { get; set; } = string.Empty;

    public string CourseCodeSnapshot { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;

    public AcademicScheduleSlotDto[] ScheduleSlots { get; set; } = [];
}
