using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using ZYC.VerbClass.Academic.Application.Contracts;
using ZYC.VerbClass.Academic.Domain;

namespace ZYC.VerbClass.Academic.Application;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AcademicDomainModule),
    typeof(AcademicApplicationContractsModule)
)]
public class AcademicApplicationModule : AbpModule
{
}
