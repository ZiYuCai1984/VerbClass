using ZYC.VerbClass.Academic.Application.Contracts.Academic;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public class CourseOfferingDetailDto
{
    public Guid Id { get; set; }

    public Guid AcademicTermId { get; set; }

    public int AcademicYear { get; set; }

    public string AcademicTermCode { get; set; } = string.Empty;

    public string AcademicTermName { get; set; } = string.Empty;

    public Guid CourseDefinitionId { get; set; }

    public string OfferingCode { get; set; } = string.Empty;

    public string CourseCodeSnapshot { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;

    public AcademicScheduleSlotDto[] ScheduleSlots { get; set; } = [];

    public AcademicPeriodDefinitionDto[] TermPeriods { get; set; } = [];
}
