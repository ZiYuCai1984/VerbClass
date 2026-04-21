using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts;

[DependsOn(
    typeof(AcademicDomainSharedModule)
)]
public class AcademicApplicationContractsModule : AbpModule
{
}
