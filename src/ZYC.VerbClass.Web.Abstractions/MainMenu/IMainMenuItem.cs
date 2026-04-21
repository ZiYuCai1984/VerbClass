namespace ZYC.VerbClass.Web.Abstractions.MainMenu;

public interface IMainMenuItem
{
    string Href { get; }

    IMainMenuItem[] SubItems { get; }

    string Title { get; }

    string? Icon { get; }

    string Anchor { get; }

    int Priority { get; }

    bool Localization { get; }

    Task<bool> IsVisibleAsync(UserTenantContext context);

}