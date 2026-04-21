using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.MyCourses;

public interface IMyCourseAppService : IApplicationService
{
    Task<MyCourseListItemDto[]> GetListAsync();
}
