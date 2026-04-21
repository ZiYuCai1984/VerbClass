using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;
using ZYC.VerbClass.Domain.Shared.Localization;

namespace ZYC.VerbClass.Web;

[Dependency(ReplaceServices = true)]
public class VerbClassBrandingProvider : DefaultBrandingProvider
{
    private readonly　IStringLocalizer<VerbClassResource> _localizer;

    public VerbClassBrandingProvider(IStringLocalizer<VerbClassResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer[nameof(AppName)];
}