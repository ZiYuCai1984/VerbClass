using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace ZYC.VerbClass.Application.Contracts.UserProfiles;

public interface IUserAvatarAppService : IApplicationService
{
    Task UploadAsync(Guid userId, IRemoteStreamContent avatar);

    Task<IRemoteStreamContent?> GetAsync(Guid userId);

    Task RemoveAsync(Guid userId);
}
