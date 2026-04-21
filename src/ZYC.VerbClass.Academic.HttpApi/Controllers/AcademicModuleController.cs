using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using ZYC.VerbClass.Academic.Application.Contracts;

namespace ZYC.VerbClass.Academic.HttpApi.Controllers;

[Area(AcademicRemoteServiceConsts.ModuleName)]
[Route("api/academic/module")]
public class AcademicModuleController : AbpControllerBase
{
    private readonly IAcademicModuleAppService _academicModuleAppService;

    public AcademicModuleController(IAcademicModuleAppService academicModuleAppService)
    {
        _academicModuleAppService = academicModuleAppService;
    }

    [HttpGet("status")]
    public Task<string> GetStatusAsync()
    {
        return _academicModuleAppService.GetStatusAsync();
    }
}
