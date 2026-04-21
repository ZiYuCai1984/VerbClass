using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;

public interface ICourseDefinitionAppService : IApplicationService
{
    Task<CourseDefinitionListItemDto[]> GetListAsync();

    Task<CourseDefinitionCommandResultDto> CreateAsync(CreateCourseDefinitionInput input);

    Task<CourseDefinitionCommandResultDto> UpdateAsync(Guid id, UpdateCourseDefinitionInput input);

    Task<CourseDefinitionCommandResultDto> DeleteAsync(Guid id);
}
