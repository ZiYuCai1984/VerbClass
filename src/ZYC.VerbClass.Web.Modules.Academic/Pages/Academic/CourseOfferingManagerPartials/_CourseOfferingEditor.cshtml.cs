using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingEditorModel : CourseOfferingEditorInput
{
    public bool IsCreate { get; set; }

    public Guid? CourseOfferingId { get; set; }

    public string AcademicTermTitle { get; set; } = string.Empty;

    public string CourseCodeSnapshot { get; set; } = string.Empty;

    public string CourseNameSnapshot { get; set; } = string.Empty;

    public CourseDefinitionOptionModel[] CourseDefinitionOptions { get; set; } = [];

    public CourseOfferingTermPeriodOptionModel[] TermPeriods { get; set; } = [];

    public AcademicWeekdayOptionModel[] WeekdayOptions { get; set; } = [];

    public string SubmitText => IsCreate ? "Create Offering" : "Save Changes";

    public string DetailTitle => IsCreate ? "New Offering" : "Edit Offering";
}
