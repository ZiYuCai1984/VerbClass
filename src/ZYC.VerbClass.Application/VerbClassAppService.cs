using Volo.Abp.Application.Services;
using ZYC.VerbClass.Domain.Shared.Localization;

namespace ZYC.VerbClass.Application;

/* Inherit your application services from this class.
 */
public abstract class VerbClassAppService : ApplicationService
{
    protected VerbClassAppService()
    {
        LocalizationResource = typeof(VerbClassResource);
    }
}
