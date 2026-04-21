namespace ZYC.VerbClass.Web.Abstractions.MainMenu;

public interface IMainMenuItemsProvider : IMainMenuItem
{
    void RegisterItem(IMainMenuItem item);

    void RegisterItem<T>() where T : IMainMenuItem;
}