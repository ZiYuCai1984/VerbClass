using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Mock;

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class MockModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(MockModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MockModule>();
        });
    }

    public override Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        context.ServiceProvider.RegisterRootMenuItem<MockMainMenuItem>();
        return base.OnPostApplicationInitializationAsync(context);
    }
}