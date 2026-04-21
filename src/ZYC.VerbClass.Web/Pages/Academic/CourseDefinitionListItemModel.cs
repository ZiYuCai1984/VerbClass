namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseDefinitionListItemModel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public bool IsActive { get; set; }

    public int OfferingCount { get; set; }

    public int ActiveOfferingCount { get; set; }

    public int LockedOfferingCount { get; set; }
}
