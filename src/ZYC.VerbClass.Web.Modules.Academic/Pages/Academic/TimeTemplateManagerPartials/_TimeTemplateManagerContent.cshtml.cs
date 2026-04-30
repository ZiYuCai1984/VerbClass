namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimeTemplateManagerPartials;

public class TimeTemplateManagerContentModel
{
    public TimeTemplateListItem[] Templates { get; init; } = [];

    public Guid? ActiveTemplateId { get; init; }

    public TimeTemplateInfoModel? SelectedTemplate { get; init; }

    public TimeTemplateEditorModel? Editor { get; init; }

    public string DetailTitle =>
        Editor is not null ? Editor.DetailTitle :
        SelectedTemplate is not null ? "Template Details" :
        "Details";
}
