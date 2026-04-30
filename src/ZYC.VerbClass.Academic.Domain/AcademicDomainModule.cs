using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AcademicDomainSharedModule)
)]
public class AcademicDomainModule : AbpModule
{
}
