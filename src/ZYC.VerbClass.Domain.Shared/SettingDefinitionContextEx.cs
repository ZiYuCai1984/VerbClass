using System.Diagnostics;
using Volo.Abp.Settings;

namespace ZYC.VerbClass.Domain.Shared;

public static class SettingDefinitionContextEx
{
    public static SettingDefinition Get(this ISettingDefinitionContext context, string name)
    {
        var definition = context.GetOrNull(name);
        Debug.Assert(definition != null);

        return definition;
    }
}