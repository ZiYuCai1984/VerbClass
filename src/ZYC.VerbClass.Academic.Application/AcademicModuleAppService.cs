using Volo.Abp.Application.Services;
using ZYC.VerbClass.Academic.Application.Contracts;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application;

public class AcademicModuleAppService : ApplicationService, IAcademicModuleAppService
{
    public Task<string> GetStatusAsync()
    {
        return Task.FromResult($"{AcademicConsts.ModuleName} module is ready.");
    }
}
