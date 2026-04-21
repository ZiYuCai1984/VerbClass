using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Application.Contracts.UserProfiles;

public interface IUserProfileAppService : IApplicationService
{
    Task<UserProfileDto> GetAsync();
}
