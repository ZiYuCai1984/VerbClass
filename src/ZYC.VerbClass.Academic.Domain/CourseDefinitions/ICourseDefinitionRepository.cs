using Volo.Abp.Domain.Repositories;

namespace ZYC.VerbClass.Academic.Domain.CourseDefinitions;

public interface ICourseDefinitionRepository : IRepository<CourseDefinition, Guid>
{
    Task<CourseDefinition?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<bool> IsCodeExistsAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
