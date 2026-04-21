using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public interface ICourseOfferingAppService : IApplicationService
{
    Task<CourseOfferingListItemDto[]> GetListAsync();

    Task<CourseOfferingCommandResultDto> CreateAsync(CreateCourseOfferingInput input);

    Task<CourseOfferingCommandResultDto> UpdateAsync(Guid id, UpdateCourseOfferingInput input);

    Task<CourseOfferingCommandResultDto> DeleteAsync(Guid id);

    Task<CourseOfferingCommandResultDto> LockAsync(Guid id);

    Task<CourseOfferingCommandResultDto> UnlockAsync(Guid id);
}
