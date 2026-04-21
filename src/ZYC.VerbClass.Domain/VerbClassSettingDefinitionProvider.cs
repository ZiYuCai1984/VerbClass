using Volo.Abp.Settings;

namespace ZYC.VerbClass.Domain;

public class VerbClassSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(VerbClassSettings.MySetting1));
    }
}
