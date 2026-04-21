using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts;

public interface IAcademicModuleAppService : IApplicationService
{
    Task<string> GetStatusAsync();
}
