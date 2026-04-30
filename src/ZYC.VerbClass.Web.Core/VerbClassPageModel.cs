using Microsoft.AspNetCore.Mvc.Filters;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using ZYC.VerbClass.Domain.Shared.Localization;

namespace ZYC.VerbClass.Web.Core;

public abstract partial class VerbClassPageModel : AbpPageModel
{
    protected VerbClassPageModel(ILifetimeScope lifetimeScope)
    {
        LifetimeScope = lifetimeScope;

        LocalizationResourceType = typeof(VerbClassResource);
    }

    protected ILifetimeScope LifetimeScope { get; }

    protected virtual string PageTitle => "";

    //TODO-zyc PartialViewAsync


    //public async Task<PartialViewResult> PartialViewAsync(VerbClassModel model)
    //{
    //    var typeName = model.GetType().Name;
    //    var partialViewName = $"_{typeName.RemovePostFix("Model")}";
    //    await model.InitializeAsync();
    //    return PartialView(partialViewName, model);
    //}

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        ViewData["Title"] = PageTitle;
        base.OnPageHandlerExecuting(context);
    }
}