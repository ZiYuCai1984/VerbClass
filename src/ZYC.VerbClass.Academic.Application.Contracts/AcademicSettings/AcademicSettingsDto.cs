namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;

public class AcademicSettingsDto
{
    public Guid? CurrentTermId { get; set; }

    public AcademicSettingsTermOptionDto[] TermOptions { get; set; } = [];

    public AcademicSettingsPermissionsDto Permissions { get; set; } = new();
}
