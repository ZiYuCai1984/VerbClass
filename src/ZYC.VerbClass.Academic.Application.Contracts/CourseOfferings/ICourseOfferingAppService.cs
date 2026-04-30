using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public interface ICourseOfferingAppService : IApplicationService
{
    Task<CourseOfferingListItemDto[]> GetListAsync(Guid academicTermId);

    Task<CourseOfferingDetailDto> GetAsync(Guid courseOfferingId);

    Task<CourseOfferingCommandResultDto> CreateAsync(CreateCourseOfferingInput input);

    Task<CourseOfferingCommandResultDto> UpdateAsync(Guid courseOfferingId, UpdateCourseOfferingInput input);

    Task<CourseOfferingCommandResultDto> DeleteAsync(Guid courseOfferingId);
}
