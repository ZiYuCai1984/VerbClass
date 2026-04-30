using Volo.Abp.Domain.Repositories;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;

public interface ICourseOfferingParticipantRepository : IRepository<CourseOfferingParticipant, Guid>
{
    Task<CourseOfferingParticipant[]> GetListByOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default);

    Task<CourseOfferingParticipant[]> GetListByOfferingsAsync(
        IReadOnlyCollection<Guid> courseOfferingIds,
        CancellationToken cancellationToken = default);

    Task<bool> IsUserAssignedAsync(
        Guid courseOfferingId,
        Guid userId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasCourseOfferingAsync(
        Guid courseOfferingId,
        CancellationToken cancellationToken = default);
}
