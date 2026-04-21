using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Application.Contracts;

namespace ZYC.VerbClass.Academic.HttpApi.Client;

[DependsOn(
    typeof(AcademicApplicationContractsModule),
    typeof(AbpHttpClientModule)
)]
public class AcademicHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(AcademicApplicationContractsModule).Assembly,
            AcademicRemoteServiceConsts.RemoteServiceName
        );
    }
}
