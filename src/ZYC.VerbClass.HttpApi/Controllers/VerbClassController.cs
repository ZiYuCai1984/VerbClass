using Volo.Abp.AspNetCore.Mvc;
using ZYC.VerbClass.Domain.Shared.Localization;

namespace ZYC.VerbClass.HttpApi.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class VerbClassController : AbpControllerBase
{
    protected VerbClassController()
    {
        LocalizationResource = typeof(VerbClassResource);
    }
}
