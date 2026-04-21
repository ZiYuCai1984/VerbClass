using ZYC.VerbClass.Web.Abstractions.Toast;

namespace ZYC.VerbClass.Web.Core;

public abstract partial class VerbClassPageModel
{
    private IToastManager? _toastManager;

    private IToastManager ToastManager => _toastManager ??= LifetimeScope.Resolve<IToastManager>();

    protected void ToastInfo(string message, string? title = null)
    {
        ToastManager.Info(message, title);
    }

    protected void ToastWarn(string message, string? title = null)
    {
        ToastManager.Warn(message, title);
    }

    protected void ToastError(string message, string? title = null)
    {
        ToastManager.Error(message, title);
    }

    protected void ToastError(Exception e, string? title = null)
    {
        ToastManager.Error(e, title);
    }

    protected void QueueToast(ToastMessage toast)
    {
        ToastManager.Queue(toast);
    }
}