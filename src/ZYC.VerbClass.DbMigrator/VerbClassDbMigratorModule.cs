using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Application.Contracts;
using ZYC.VerbClass.EntityFrameworkCore;

namespace ZYC.VerbClass.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(VerbClassEntityFrameworkCoreModule),
    typeof(VerbClassApplicationContractsModule)
)]
public class VerbClassDbMigratorModule : AbpModule
{
}