using Volo.Abp.Domain.Repositories;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public interface ICourseOfferingRepository : IRepository<CourseOffering, Guid>
{
    Task<CourseOffering[]> GetListByTermAsync(
        Guid academicTermId,
        CancellationToken cancellationToken = default);

    Task<bool> IsOfferingCodeExistsAsync(
        Guid academicTermId,
        string offeringCode,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasCourseDefinitionAsync(
        Guid courseDefinitionId,
        CancellationToken cancellationToken = default);

    Task<bool> HasAcademicTermAsync(
        Guid academicTermId,
        CancellationToken cancellationToken = default);
}
