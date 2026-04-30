using Volo.Abp.Application;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(AcademicDomainSharedModule)
)]
public class AcademicApplicationContractsModule : AbpModule
{
}
