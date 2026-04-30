using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;

public interface ICourseDefinitionAppService : IApplicationService
{
    Task<CourseDefinitionListItemDto[]> GetListAsync();

    Task<CourseDefinitionDetailDto> GetAsync(Guid courseDefinitionId);

    Task<CourseDefinitionOptionDto[]> GetActiveOptionsAsync();

    Task<CourseDefinitionCommandResultDto> CreateAsync(CreateCourseDefinitionInput input);

    Task<CourseDefinitionCommandResultDto> UpdateAsync(Guid courseDefinitionId, UpdateCourseDefinitionInput input);

    Task<CourseDefinitionCommandResultDto> ActivateAsync(Guid courseDefinitionId);

    Task<CourseDefinitionCommandResultDto> DeactivateAsync(Guid courseDefinitionId);

    Task<CourseDefinitionCommandResultDto> DeleteAsync(Guid courseDefinitionId);
}
