using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Application.Contracts;

namespace ZYC.VerbClass.Academic.HttpApi.Client;

[DependsOn(
    typeof(AbpHttpClientModule),
    typeof(AcademicApplicationContractsModule)
)]
public class AcademicHttpApiClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(AcademicApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
