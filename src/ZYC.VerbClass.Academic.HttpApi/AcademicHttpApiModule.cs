using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Application;
using ZYC.VerbClass.Academic.Application.Contracts;

namespace ZYC.VerbClass.Academic.HttpApi;

[DependsOn(
    typeof(AcademicApplicationModule),
    typeof(AcademicApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class AcademicHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AcademicApplicationModule).Assembly, settings =>
            {
                settings.RootPath = "academic";
            });
        });
    }
}
