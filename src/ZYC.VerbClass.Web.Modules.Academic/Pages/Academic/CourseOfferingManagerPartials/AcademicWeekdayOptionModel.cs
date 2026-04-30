using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class AcademicWeekdayOptionModel
{
    public AcademicWeekdayOptionModel(AcademicWeekday value, string label)
    {
        Value = value;
        Label = label;
    }

    public AcademicWeekday Value { get; }

    public string Label { get; }
}
