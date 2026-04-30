namespace ZYC.VerbClass.Web.Pages.Admin.DepartmentManagerPartials;

public class DepartmentManagerContentModel
{
    public DepartmentListItem[] Departments { get; set; } = [];

    public Guid? ActiveDepartmentId { get; set; }

    public DepartmentInfoModel? SelectedDepartment { get; set; }

    public DepartmentEditorModel? Editor { get; set; }

    public bool CanCreate { get; set; }

    public string DetailTitle
    {
        get
        {
            if (Editor?.IsCreate == true)
            {
                return "New Department";
            }

            if (Editor is not null)
            {
                return "Edit Department";
            }

            return "Department Details";
        }
    }
}