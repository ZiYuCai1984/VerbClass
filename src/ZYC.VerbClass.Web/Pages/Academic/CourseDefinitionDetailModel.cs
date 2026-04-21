using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseDefinitionDetailModel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortName { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public CourseOfferingListItemDto[] Offerings { get; set; } = [];

    public int OfferingCount => Offerings.Length;

    public int ActiveOfferingCount => Offerings.Count(
        x => x.Status is CourseOfferingStatus.Active or CourseOfferingStatus.Published
    );

    public int LockedOfferingCount => Offerings.Count(x => x.IsLocked);
}
