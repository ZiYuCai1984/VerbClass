using Microsoft.AspNetCore.Mvc.ModelBinding;
using Volo.Abp.Validation;

namespace ZYC.VerbClass.Web.Core;

public static class ModelStateValidationSupport
{
    public static void ApplyAbpValidationException(
        ModelStateDictionary modelState,
        AbpValidationException ex,
        string fallbackSummaryMessage,
        string? modelPrefix = null)
    {
        AbpValidationExceptionSupport.Apply(
            ex,
            fallbackSummaryMessage,
            message => modelState.AddModelError(string.Empty, message),
            (member, message) => modelState.AddModelError(BuildKey(modelPrefix, member), message)
        );
    }

    private static string BuildKey(string? modelPrefix, string member)
    {
        return string.IsNullOrWhiteSpace(modelPrefix)
            ? member
            : $"{modelPrefix}.{member}";
    }
}
