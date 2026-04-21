using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Application.Contracts;
using ZYC.VerbClass.Academic.Domain;

namespace ZYC.VerbClass.Academic.Application;

[DependsOn(
    typeof(AcademicDomainModule),
    typeof(AcademicApplicationContractsModule)
)]
public class AcademicApplicationModule : AbpModule
{
}
