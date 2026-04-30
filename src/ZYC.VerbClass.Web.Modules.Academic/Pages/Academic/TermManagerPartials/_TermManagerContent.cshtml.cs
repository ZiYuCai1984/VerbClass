namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TermManagerPartials;

public class TermManagerContentModel
{
    public TermListItem[] Terms { get; init; } = [];

    public Guid? ActiveTermId { get; init; }

    public TermInfoModel? SelectedTerm { get; init; }

    public TermEditorModel? Editor { get; init; }

    public string DetailTitle =>
        Editor is not null ? Editor.DetailTitle :
        SelectedTerm is not null ? "Term Details" :
        "Details";
}
