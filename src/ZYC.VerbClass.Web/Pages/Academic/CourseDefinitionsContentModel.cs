namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseDefinitionsContentModel
{
    public CourseDefinitionListItemModel[] Definitions { get; set; } = [];

    public Guid? ActiveCourseDefinitionId { get; set; }

    public CourseDefinitionDetailModel? SelectedDefinition { get; set; }

    public CourseDefinitionEditorModel? Editor { get; set; }

    public int DefinitionCount => Definitions.Length;

    public int ActiveDefinitionCount => Definitions.Count(x => x.IsActive);

    public int OfferingCount => Definitions.Sum(x => x.OfferingCount);

    public int ActiveOfferingCount => Definitions.Sum(x => x.ActiveOfferingCount);

    public int LockedOfferingCount => Definitions.Sum(x => x.LockedOfferingCount);

    public string DetailTitle => Editor switch
    {
        { IsCreate: true } => "New Course Definition",
        { IsCreate: false } => "Edit Course Definition",
        _ => SelectedDefinition?.Name ?? "Course Definitions"
    };
}
