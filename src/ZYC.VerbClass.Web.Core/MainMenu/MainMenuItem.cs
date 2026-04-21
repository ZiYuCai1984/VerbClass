using ZYC.VerbClass.Web.Abstractions.MainMenu;

namespace ZYC.VerbClass.Web.Core.MainMenu;

public class MainMenuItem : IMainMenuItem
{
    public MainMenuItem()
    {
    }

    public MainMenuItem(
        string title,
        string href,
        IMainMenuItem[] subItems,
        string? icon = null,
        string anchor = "",
        int priority = 0,
        bool localization = true)
    {
        Title = title;
        Href = href;
        SubItems = subItems;
        Icon = icon;
        Anchor = anchor;
        Priority = priority;
        Localization = localization;
    }

    public virtual string Href { get; } = "";

    public virtual IMainMenuItem[] SubItems { get; } = [];

    public virtual string Title { get; } = "";

    public virtual string? Icon { get; }

    public virtual string Anchor { get; } = "";

    public virtual int Priority { get; }

    public virtual bool Localization { get; } = true;

    public virtual async Task<bool> IsVisibleAsync(UserTenantContext context)
    {
        await Task.CompletedTask;
        return true;
    }
}