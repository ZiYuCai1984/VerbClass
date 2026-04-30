using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Academic;

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class AcademicModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(AcademicModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AcademicModule>();
        });
    }

    public override Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        context.ServiceProvider.RegisterRootMenuItem<AcademicMainMenuItem>();
        return base.OnPostApplicationInitializationAsync(context);
    }
}
