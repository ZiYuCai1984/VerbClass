using Microsoft.AspNetCore.Mvc.Rendering;

namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseOfferingModalModel : CourseOfferingCreatePageInput
{
    public bool IsCreate { get; set; } = true;

    public Guid? CourseOfferingId { get; set; }

    public string ReturnTo { get; set; } = string.Empty;

    public IReadOnlyList<SelectListItem> CourseDefinitionOptions { get; set; } = [];

    public bool CanCreateOffering => CourseDefinitionOptions.Count > 0;

    public string TitleText => IsCreate ? "Create Course Offering" : "Edit Course Offering";

    public string SubmitText => IsCreate ? "Create Course Offering" : "Save Course Offering";
}
