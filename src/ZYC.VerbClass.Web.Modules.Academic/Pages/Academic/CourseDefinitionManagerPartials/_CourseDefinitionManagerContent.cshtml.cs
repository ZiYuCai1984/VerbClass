namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseDefinitionManagerPartials;

public class CourseDefinitionManagerContentModel
{
    public CourseDefinitionListItem[] CourseDefinitions { get; set; } = [];

    public Guid? ActiveCourseDefinitionId { get; set; }

    public CourseDefinitionInfoModel? SelectedCourseDefinition { get; set; }

    public CourseDefinitionEditorModel? Editor { get; set; }

    public string DetailTitle => Editor?.DetailTitle
                                 ?? SelectedCourseDefinition?.Name
                                 ?? "Course Definition";
}
