namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;

public class AcademicSettingsTermOptionDto
{
    public Guid Id { get; set; }

    public int AcademicYear { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayName => $"{AcademicYear} {Name} ({Code})";
}
