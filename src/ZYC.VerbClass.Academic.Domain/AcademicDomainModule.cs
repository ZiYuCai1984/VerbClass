using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain;

[DependsOn(
    typeof(AcademicDomainSharedModule)
)]
public class AcademicDomainModule : AbpModule
{
}
