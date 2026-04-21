using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Pages.Academic;

[Authorize]
public class CourseManagerModel : VerbClassPageModel
{
    public CourseManagerModel(ILifetimeScope lifetimeScope) : base(lifetimeScope)
    {
    }

    [BindProperty(SupportsGet = true)]
    public Guid? SelectedCourseDefinitionId { get; set; }

    protected override string PageTitle => "Course Definitions";

    public IActionResult OnGet()
    {
        return RedirectToPage(
            "/Academic/CourseDefinitions",
            new
            {
                courseDefinitionId = SelectedCourseDefinitionId
            }
        );
    }
}
