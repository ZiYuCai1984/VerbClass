namespace ZYC.VerbClass.Web.Abstractions.AccountMenu;

public interface IAccountMenuManager
{
    void RegisterItem(IAccountMenuItem item);

    void RegisterItem<T>() where T : IAccountMenuItem;

    Task<IAccountMenuItem[]> GetItemsAsync(UserTenantContext context);
}