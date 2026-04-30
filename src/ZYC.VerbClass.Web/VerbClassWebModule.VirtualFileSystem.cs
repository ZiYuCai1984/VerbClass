using Volo.Abp.VirtualFileSystem;
using ZYC.VerbClass.Application;
using ZYC.VerbClass.Application.Contracts;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.HttpApi;
using ZYC.VerbClass.Web.Modules.Academic;
using ZYC.VerbClass.Web.Modules.Mock;

namespace ZYC.VerbClass.Web;

public partial class VerbClassWebModule
{
    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<VerbClassWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<VerbClassDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<VerbClassDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<VerbClassApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<VerbClassApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<VerbClassHttpApiModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.HttpApi"));
                options.FileSets.ReplaceEmbeddedByPhysical<AcademicModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.Web.Modules.Academic"));
                options.FileSets.ReplaceEmbeddedByPhysical<MockModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}ZYC.VerbClass.Web.Modules.Mock"));

                options.FileSets.ReplaceEmbeddedByPhysical<VerbClassWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }
}
