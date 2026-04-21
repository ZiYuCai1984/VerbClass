namespace ZYC.VerbClass.Web.Core;

public abstract class EditorInputBase
{
    private readonly Dictionary<string, List<string>> _fieldErrors = new(StringComparer.OrdinalIgnoreCase);

    public List<string> SummaryErrors { get; } = [];

    public bool HasErrors => SummaryErrors.Count > 0 || _fieldErrors.Count > 0;

    public void AddSummaryError(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            SummaryErrors.Add(message);
        }
    }

    public void AddFieldError(string fieldName, string message)
    {
        if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (!_fieldErrors.TryGetValue(fieldName, out var messages))
        {
            messages = [];
            _fieldErrors[fieldName] = messages;
        }

        messages.Add(message);
    }

    public IReadOnlyList<string> GetErrors(string fieldName)
    {
        return _fieldErrors.TryGetValue(fieldName, out var messages)
            ? messages
            : [];
    }
}