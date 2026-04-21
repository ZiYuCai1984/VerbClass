using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.EntityFrameworkCore;

public static class VerbClassEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new();

    public static void Configure()
    {
        VerbClassGlobalFeatureConfigurator.Configure();
        VerbClassModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
            //ObjectExtensionManager.Instance
            //    .MapEfCoreProperty<IdentityUser, string>(
            //        "EmployeeNo",
            //        (_, propertyBuilder) =>
            //        {
            //            propertyBuilder.HasMaxLength(32);
            //        });
        });
    }
}