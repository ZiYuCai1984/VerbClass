namespace ZYC.VerbClass.Web.Abstractions.Toast;

public class ToastMessage
{
    public ToastMessage(string type, string message, string? title = null)
    {
        Type = type;
        Message = message;
        Title = title;
    }

    public string Type { get; }

    public string Message { get; }

    public string? Title { get; }


    public static ToastMessage Warn(string message, string? title = null)
    {
        return new ToastMessage("warn", message, title);
    }

    public static ToastMessage Info(string message, string? title = null)
    {
        return new ToastMessage("info", message, title);
    }

    public static ToastMessage Error(string message, string? title = null)
    {
        return new ToastMessage("error", message, title);
    }

    public static ToastMessage Error(Exception exception, string? title = null)
    {
        return new ToastMessage("error", exception.ToString(), title);
    }
}