namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;

public class AcademicTimetableOfferingDto
{
    public Guid Id { get; set; }

    public string OfferingCode { get; set; } = string.Empty;

    public string CourseCodeSnapshot { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;

    public string[] TeacherNames { get; set; } = [];
}
