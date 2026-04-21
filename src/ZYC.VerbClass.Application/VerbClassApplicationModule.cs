using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using ZYC.VerbClass.Academic.Application;
using ZYC.VerbClass.Application.Contracts;
using ZYC.VerbClass.Domain;

namespace ZYC.VerbClass.Application;

[DependsOn(
    typeof(AcademicApplicationModule),
    typeof(VerbClassDomainModule),
    typeof(VerbClassApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class VerbClassApplicationModule : AbpModule
{

}
