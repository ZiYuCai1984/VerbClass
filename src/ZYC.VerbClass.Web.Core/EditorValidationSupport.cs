using System.ComponentModel.DataAnnotations;

using Volo.Abp.Validation;

namespace ZYC.VerbClass.Web.Core;

public static class EditorValidationSupport
{
    public static void ApplyDataAnnotations(EditorInputBase editor)
    {
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(editor, new ValidationContext(editor), validationResults, true);

        foreach (var validationResult in validationResults)
        {
            var members = validationResult.MemberNames as string[] ?? validationResult.MemberNames.ToArray();
            if (members.Length == 0)
            {
                editor.AddSummaryError(validationResult.ErrorMessage ?? "Validation failed.");
                continue;
            }

            foreach (var member in members)
            {
                editor.AddFieldError(member, validationResult.ErrorMessage ?? "Validation failed.");
            }
        }
    }

    public static void ApplyAbpValidationException(
        EditorInputBase editor,
        AbpValidationException ex,
        string fallbackSummaryMessage)
    {
        AbpValidationExceptionSupport.Apply(
            ex,
            fallbackSummaryMessage,
            editor.AddSummaryError,
            editor.AddFieldError
        );
    }
}
