using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using ZYC.VerbClass.Academic.Domain;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

[DependsOn(
    typeof(AcademicDomainModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class AcademicEntityFrameworkCoreModule : AbpModule
{
}
