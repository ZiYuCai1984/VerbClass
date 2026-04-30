using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Application.IdentityUsers;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.Application.UserProfiles;

[Authorize]
public class UserProfileAppService : VerbClassAppService, IUserProfileAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<UserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public UserProfileAppService(
        IRepository<UserProfile, Guid> userProfileRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IdentityUserManager userManager)
    {
        _userProfileRepository = userProfileRepository;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<UserProfileDto> GetAsync()
    {
        var user = await GetCurrentUserAsync();
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == user.Id);

        return VerbClassApplicationMappers.ToUserProfileDto(
            user,
            profile,
            (await _userManager.GetRolesAsync(user)).ToArray()
        );
    }

    private async Task<IdentityUser> GetCurrentUserAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException();
        }

        return await _userRepository.FindAsync(CurrentUser.Id.Value)
               ?? throw new UserFriendlyException("Current user was not found.");
    }
}
