using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ZYC.VerbClass.Application.Contracts.UserProfiles;

public interface IUserSettingsAppService : IApplicationService
{
    Task<UserSettingsDto> GetAsync();

    Task SaveAsync(UpdateUserSettingsInput input);

    Task UploadAvatarAsync(IRemoteStreamContent avatar);

    Task RemoveAvatarAsync();
}
