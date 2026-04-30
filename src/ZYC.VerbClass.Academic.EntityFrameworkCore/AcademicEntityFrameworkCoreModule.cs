using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Academic.Domain;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

[DependsOn(
    typeof(AcademicDomainModule),
    typeof(VerbClassDomainModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class AcademicEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AcademicDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlite();
        });
    }
}
