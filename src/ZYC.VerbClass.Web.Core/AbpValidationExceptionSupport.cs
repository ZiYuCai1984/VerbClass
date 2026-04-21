using Volo.Abp.Validation;

namespace ZYC.VerbClass.Web.Core;

public static class AbpValidationExceptionSupport
{
    public static void Apply(
        AbpValidationException ex,
        string fallbackSummaryMessage,
        Action<string> addSummaryError,
        Action<string, string> addFieldError)
    {
        var validationErrors = ex.ValidationErrors.ToArray();

        if (validationErrors.Length == 0)
        {
            addSummaryError(string.IsNullOrWhiteSpace(ex.Message) ? fallbackSummaryMessage : ex.Message);
            return;
        }

        foreach (var validationError in validationErrors)
        {
            var message = string.IsNullOrWhiteSpace(validationError.ErrorMessage)
                ? string.IsNullOrWhiteSpace(ex.Message)
                    ? "Validation failed."
                    : ex.Message
                : validationError.ErrorMessage;
            var members = validationError.MemberNames as string[] ?? validationError.MemberNames.ToArray();

            if (members.Length == 0)
            {
                addSummaryError(message);
                continue;
            }

            foreach (var member in members)
            {
                addFieldError(member, message);
            }
        }
    }
}
