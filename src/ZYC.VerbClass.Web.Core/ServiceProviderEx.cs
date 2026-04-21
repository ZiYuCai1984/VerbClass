using Microsoft.Extensions.DependencyInjection;
using ZYC.VerbClass.Web.Abstractions.MainMenu;

namespace ZYC.VerbClass.Web.Core;

public static class ServiceProviderEx
{
    public static void RegisterRootMenuItem<T>(this IServiceProvider serviceProvider) where T : IMainMenuItem
    {
        serviceProvider.GetRequiredService<IMainMenuManager>().RegisterItem<T>();
    }
}