using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Threading;
using ZYC.VerbClass.Application.Contracts.UserProfiles;
using ZYC.VerbClass.Domain.AppFiles;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Application.UserProfiles;

[Authorize]
public class UserAvatarAppService : VerbClassAppService, IUserAvatarAppService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/gif",
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gif",
        ".jpeg",
        ".jpg",
        ".png",
        ".webp"
    };

    private const long MaxAvatarSize = 2 * 1024 * 1024;

    private readonly IBlobContainer<UserAvatarBlobContainer> _blobContainer;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AppFile, Guid> _appFileRepository;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<Domain.UserProfiles.UserProfile, Guid> _userProfileRepository;
    private readonly UserAvatarContentProvider _userAvatarContentProvider;

    public UserAvatarAppService(
        IBlobContainer<UserAvatarBlobContainer> blobContainer,
        IGuidGenerator guidGenerator,
        IRepository<AppFile, Guid> appFileRepository,
        ICancellationTokenProvider cancellationTokenProvider,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<Domain.UserProfiles.UserProfile, Guid> userProfileRepository,
        UserAvatarContentProvider userAvatarContentProvider)
    {
        _blobContainer = blobContainer;
        _guidGenerator = guidGenerator;
        _appFileRepository = appFileRepository;
        _cancellationTokenProvider = cancellationTokenProvider;
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
        _userAvatarContentProvider = userAvatarContentProvider;
    }

    public async Task UploadAsync(Guid userId, IRemoteStreamContent avatar)
    {
        Check.NotNull(avatar, nameof(avatar));
        await EnsureCanManageAsync(userId);

        var tenantId = CurrentTenant.Id ?? throw new UserFriendlyException(
            "Avatar upload is only available inside a tenant."
        );

        var user = await _userRepository.FindAsync(userId);
        if (user is null)
        {
            throw new UserFriendlyException("User was not found.");
        }

        var originalFileName = Path.GetFileName(avatar.FileName);
        if (originalFileName.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("Avatar file name is required.");
        }

        var extension = Path.GetExtension(originalFileName);
        if (!AllowedFileExtensions.Contains(extension))
        {
            throw new UserFriendlyException("Only JPG, PNG, WEBP, and GIF avatars are supported.");
        }

        var contentType = NormalizeContentType(avatar.ContentType, extension);
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new UserFriendlyException("Only JPG, PNG, WEBP, and GIF avatars are supported.");
        }

        await using var inputStream = avatar.GetStream();
        await using var bufferedStream = new MemoryStream();
        await inputStream.CopyToAsync(bufferedStream, _cancellationTokenProvider.Token);

        if (bufferedStream.Length == 0)
        {
            throw new UserFriendlyException("Avatar file is empty.");
        }

        if (bufferedStream.Length > MaxAvatarSize)
        {
            throw new UserFriendlyException("Avatar file size must not exceed 2 MB.");
        }

        bufferedStream.Position = 0;

        var (profile, isNewProfile) = await GetOrCreateProfileAsync(userId, tenantId);
        var previousFile = profile.AvatarFileId.HasValue
            ? await _appFileRepository.FindAsync(profile.AvatarFileId.Value)
            : null;

        var appFile = new AppFile(
            _guidGenerator.Create(),
            tenantId,
            originalFileName,
            contentType,
            bufferedStream.Length,
            CreateBlobName(userId, extension)
        );

        await _blobContainer.SaveAsync(
            appFile.BlobName,
            bufferedStream,
            overrideExisting: false,
            _cancellationTokenProvider.Token
        );

        await _appFileRepository.InsertAsync(appFile, true);

        profile.ChangeAvatar(appFile.Id);

        if (isNewProfile)
        {
            await _userProfileRepository.InsertAsync(profile, true);
        }
        else
        {
            await _userProfileRepository.UpdateAsync(profile, true);
        }

        await DeletePreviousAvatarAsync(previousFile);
    }

    public async Task<IRemoteStreamContent?> GetAsync(Guid userId)
    {
        await EnsureCanReadAsync(userId);
        return await _userAvatarContentProvider.GetAsync(userId);
    }

    public async Task RemoveAsync(Guid userId)
    {
        await EnsureCanManageAsync(userId);

        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        if (profile?.AvatarFileId is not Guid avatarFileId)
        {
            return;
        }

        var appFile = await _appFileRepository.FindAsync(avatarFileId);

        profile.ChangeAvatar(null);
        await _userProfileRepository.UpdateAsync(profile, true);

        await DeletePreviousAvatarAsync(appFile);
    }

    private async Task<(Domain.UserProfiles.UserProfile Profile, bool IsNew)> GetOrCreateProfileAsync(
        Guid userId,
        Guid tenantId)
    {
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        if (profile is not null)
        {
            return (profile, false);
        }

        return (new Domain.UserProfiles.UserProfile(_guidGenerator.Create(), tenantId, userId), true);
    }

    private async Task DeletePreviousAvatarAsync(AppFile? appFile)
    {
        if (appFile is null)
        {
            return;
        }

        await _blobContainer.DeleteAsync(appFile.BlobName, _cancellationTokenProvider.Token);
        await _appFileRepository.DeleteAsync(appFile, true);
    }

    private async Task EnsureCanReadAsync(Guid userId)
    {
        if (CurrentUser.Id == userId)
        {
            return;
        }

        if (!await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.Access))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task EnsureCanManageAsync(Guid userId)
    {
        if (CurrentUser.Id == userId)
        {
            return;
        }

        if (!await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Users.Update))
        {
            throw new AbpAuthorizationException();
        }
    }

    private static string CreateBlobName(Guid userId, string extension)
    {
        return $"avatars/{userId:D}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
    }

    private static string NormalizeContentType(string? contentType, string extension)
    {
        if (!contentType.IsNullOrWhiteSpace())
        {
            return contentType.Trim();
        }

        return extension.ToLowerInvariant() switch
        {
            ".gif" => "image/gif",
            ".jpeg" => "image/jpeg",
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => string.Empty
        };
    }

}
