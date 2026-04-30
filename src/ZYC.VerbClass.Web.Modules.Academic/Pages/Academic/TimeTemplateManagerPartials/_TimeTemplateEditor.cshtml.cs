namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

public class TimeTemplateEditorModel : TimeTemplateEditorInput
{
    public bool IsCreate { get; set; }

    public bool IsCopy { get; set; }

    public Guid? TemplateId { get; set; }

    public string SubmitText => IsCreate ? "Create Template" : "Save Changes";

    public string DetailTitle =>
        IsCreate ? "New Template" :
        IsCopy ? "Copy Template" :
        "Edit Template";
}
