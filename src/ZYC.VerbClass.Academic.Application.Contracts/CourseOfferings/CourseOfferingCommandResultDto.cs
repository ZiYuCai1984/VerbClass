namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public class CourseOfferingCommandResultDto
{
    public Guid Id { get; set; }

    public Guid AcademicTermId { get; set; }

    public string OfferingCode { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;
}
