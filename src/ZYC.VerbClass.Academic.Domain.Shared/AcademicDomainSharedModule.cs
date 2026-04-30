using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.Shared;

[DependsOn(
    typeof(AbpDddDomainSharedModule),
    typeof(VerbClassDomainSharedModule)
)]
public class AcademicDomainSharedModule : AbpModule
{
}
