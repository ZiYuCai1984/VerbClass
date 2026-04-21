using Autofac;
using Microsoft.AspNetCore.Mvc;
using ZYC.VerbClass.Web.Core;

namespace ZYC.VerbClass.Web.Modules.Mock.Pages;

public class MockModel : VerbClassPageModel
{
    public MockModel(ILifetimeScope lifetimeScope) : base(lifetimeScope)
    {
    }

    public void OnGet()
    {
    }

    public PartialViewResult OnGetModalDemo()
    {
        return PartialView("_MockModalDemo", new object());
    }
}
