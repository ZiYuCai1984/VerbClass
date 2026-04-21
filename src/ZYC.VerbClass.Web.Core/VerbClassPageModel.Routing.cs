using Autofac.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ZYC.VerbClass.Web.Core;

public abstract partial class VerbClassPageModel
{
    protected async Task<TModel> ResolveModelAsync<TModel>(params Parameter[] parameters)
        where TModel : VerbClassModel
    {
        var model = parameters.Length == 0
            ? LifetimeScope.Resolve<TModel>()
            : LifetimeScope.Resolve<TModel>(parameters);

        await model.InitializeAsync();
        return model;
    }

    protected void SetReplaceUrl(string routeKey, Guid? routeValue = null)
    {
        Response.Headers["HX-Replace-Url"] = routeValue.HasValue
            ? Url.Page(
                null,
                null,
                new RouteValueDictionary
                {
                    [routeKey] = routeValue.Value
                })!
            : Url.Page(null)!;
    }

    protected void SetReplaceUrl(string routeKey, string? routeValue)
    {
        Response.Headers["HX-Replace-Url"] = !string.IsNullOrWhiteSpace(routeValue)
            ? Url.Page(
                null,
                null,
                new RouteValueDictionary
                {
                    [routeKey] = routeValue
                })!
            : Url.Page(null)!;
    }
}
