using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Application.Contracts;

namespace ZYC.VerbClass.Academic.HttpApi;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(AcademicApplicationContractsModule)
)]
public class AcademicHttpApiModule : AbpModule
{
}
