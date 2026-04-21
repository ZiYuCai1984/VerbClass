namespace ZYC.VerbClass.Web.Pages.Academic;

public class CourseOfferingsContentModel
{
    public CourseOfferingListItemModel[] Offerings { get; set; } = [];

    public Guid? ActiveCourseOfferingId { get; set; }

    public CourseOfferingDetailModel? SelectedOffering { get; set; }

    public int OfferingCount => Offerings.Length;

    public int ActiveOfferingCount => Offerings.Count(
        x => x.Status is ZYC.VerbClass.Academic.Domain.Shared.CourseOfferingStatus.Active
            or ZYC.VerbClass.Academic.Domain.Shared.CourseOfferingStatus.Published
    );

    public int LockedOfferingCount => Offerings.Count(x => x.IsLocked);

    public int CourseDefinitionCount => Offerings
        .Select(x => x.CourseDefinitionId)
        .Distinct()
        .Count();
}
