using Volo.Abp.Settings;

namespace ZYC.VerbClass.Academic.Domain;

public class AcademicSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(AcademicSettings.CurrentTermId, string.Empty));
    }
}
