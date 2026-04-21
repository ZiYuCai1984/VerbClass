using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Volo.Abp.Users;
using ZYC.VerbClass.Application.Contracts.UserProfiles;

namespace ZYC.VerbClass.Web.Pages.Account;

[Authorize]
public class AvatarModel : VerbClassPageModel
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserAvatarAppService _userAvatarAppService;

    public AvatarModel(
        ILifetimeScope lifetimeScope,
        ICurrentUser currentUser,
        IUserAvatarAppService userAvatarAppService) : base(lifetimeScope)
    {
        _currentUser = currentUser;
        _userAvatarAppService = userAvatarAppService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (_currentUser.Id is not Guid userId)
        {
            return NotFound();
        }

        var avatar = await _userAvatarAppService.GetAsync(userId);
        if (avatar is null)
        {
            return NotFound();
        }

        Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, max-age=0";
        Response.Headers[HeaderNames.Pragma] = "no-cache";
        Response.Headers[HeaderNames.Expires] = "0";

        var contentType = string.IsNullOrWhiteSpace(avatar.ContentType)
            ? "image/png"
            : avatar.ContentType;

        return File(avatar.GetStream(), contentType);
    }
}
