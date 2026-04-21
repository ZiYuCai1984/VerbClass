using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZYC.VerbClass.Web.Pages;

[AllowAnonymous]
public class ErrorModel : PageModel
{
    public string? RequestId { get; private set; }

    public void OnGet()
    {
        RequestId =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Path ??
            HttpContext.TraceIdentifier;
    }
}
