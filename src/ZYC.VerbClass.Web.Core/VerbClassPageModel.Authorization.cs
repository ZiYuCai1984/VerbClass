using Microsoft.AspNetCore.Mvc;

namespace ZYC.VerbClass.Web.Core;

public abstract partial class VerbClassPageModel
{
    private const string HtmxRequestHeaderName = "HX-Request";
    private const string HtmxRedirectHeaderName = "HX-Redirect";
    private const string AccessDeniedPagePath = Routes.Account_AccessDenied;

    protected IActionResult ForbidOrHtmxRedirectToAccessDenied()
    {
        if (!Request.Headers.ContainsKey(HtmxRequestHeaderName))
        {
            return Forbid();
        }

        Response.Headers[HtmxRedirectHeaderName] = Url.Page(AccessDeniedPagePath) ?? AccessDeniedPagePath;
        return new EmptyResult();
    }
}
