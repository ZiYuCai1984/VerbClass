namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

public class CourseOfferingManagerContentModel
{
    public CourseOfferingTermOptionModel[] Terms { get; set; } = [];

    public Guid? ActiveTermId { get; set; }

    public CourseOfferingListItem[] Offerings { get; set; } = [];

    public Guid? ActiveOfferingId { get; set; }

    public CourseOfferingInfoModel? SelectedOffering { get; set; }

    public CourseOfferingEditorModel? Editor { get; set; }

    public bool CanCreateOffering => ActiveTermId.HasValue;

    public string DetailTitle => Editor?.DetailTitle
                                 ?? SelectedOffering?.OfferingCode
                                 ?? "Course Offering";
}
