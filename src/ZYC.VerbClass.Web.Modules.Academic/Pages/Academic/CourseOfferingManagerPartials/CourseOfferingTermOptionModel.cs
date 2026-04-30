namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingTermOptionModel
{
    public CourseOfferingTermOptionModel(
        Guid id,
        int academicYear,
        string code,
        string name)
    {
        Id = id;
        AcademicYear = academicYear;
        Code = code;
        Name = name;
    }

    public Guid Id { get; }

    public int AcademicYear { get; }

    public string Code { get; }

    public string Name { get; }

    public string Title => $"{AcademicYear} {Name}";

    public string DisplayName => $"{Title} ({Code})";
}
