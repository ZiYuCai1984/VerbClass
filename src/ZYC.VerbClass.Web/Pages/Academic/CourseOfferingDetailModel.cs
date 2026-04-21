namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseOfferingDetailModel : CourseOfferingListItemModel
{
    public string? CourseDefinitionShortName { get; set; }

    public string? CourseDefinitionDescription { get; set; }

    public bool IsCourseDefinitionActive { get; set; }

    public DateTime EnrollmentStartsAt { get; set; }

    public DateTime EnrollmentEndsAt { get; set; }

    public string EnrollmentOpenLabel => EnrollmentStartsAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    public string EnrollmentCloseLabel => EnrollmentEndsAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
}
