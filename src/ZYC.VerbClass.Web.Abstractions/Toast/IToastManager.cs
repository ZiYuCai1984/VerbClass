namespace ZYC.VerbClass.Web.Abstractions.Toast;

public interface IToastManager
{
    void Queue(ToastMessage toast);

    void Info(string message, string? title = null);

    void Warn(string message, string? title = null);

    void Error(string message, string? title = null);

    void Error(Exception exception, string? title = null);
}
