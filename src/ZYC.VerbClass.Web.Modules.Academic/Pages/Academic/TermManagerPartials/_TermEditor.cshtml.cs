namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

public class TermEditorModel : TermEditorInput
{
    public bool IsCreate { get; set; }

    public Guid? TermId { get; set; }

    public bool IsLocked { get; set; }

    public TemplateOptionModel[] TemplateOptions { get; set; } = [];

    public PeriodRowModel[] Periods { get; set; } = [];

    public string SubmitText => IsCreate ? "Create Term" : "Save Changes";

    public string DetailTitle => IsCreate ? "New Term" : "Edit Term";
}
