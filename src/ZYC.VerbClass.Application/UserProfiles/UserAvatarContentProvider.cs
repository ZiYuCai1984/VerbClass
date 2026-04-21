using System.Text;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Threading;
using ZYC.VerbClass.Domain.AppFiles;

namespace ZYC.VerbClass.Application.UserProfiles;

public class UserAvatarContentProvider : ITransientDependency
{
    private readonly IBlobContainer<UserAvatarBlobContainer> _blobContainer;
    private readonly IRepository<AppFile, Guid> _appFileRepository;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<Domain.UserProfiles.UserProfile, Guid> _userProfileRepository;

    public UserAvatarContentProvider(
        IBlobContainer<UserAvatarBlobContainer> blobContainer,
        IRepository<AppFile, Guid> appFileRepository,
        ICancellationTokenProvider cancellationTokenProvider,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<Domain.UserProfiles.UserProfile, Guid> userProfileRepository)
    {
        _blobContainer = blobContainer;
        _appFileRepository = appFileRepository;
        _cancellationTokenProvider = cancellationTokenProvider;
        _userRepository = userRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<IRemoteStreamContent?> GetAsync(Guid userId)
    {
        var user = await _userRepository.FindAsync(userId);
        if (user is null)
        {
            return null;
        }

        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        if (profile?.AvatarFileId is not Guid avatarFileId)
        {
            return CreateDefaultAvatar(user);
        }

        var appFile = await _appFileRepository.FindAsync(avatarFileId);
        if (appFile is null)
        {
            return CreateDefaultAvatar(user);
        }

        var stream = await _blobContainer.GetOrNullAsync(appFile.BlobName, _cancellationTokenProvider.Token);
        if (stream is null)
        {
            return CreateDefaultAvatar(user);
        }

        return new RemoteStreamContent(stream, appFile.FileName, appFile.ContentType);
    }

    private static RemoteStreamContent CreateDefaultAvatar(IdentityUser user)
    {
        var fileName = string.IsNullOrWhiteSpace(user.UserName)
            ? $"{user.Id:N}.svg"
            : $"{user.UserName}.svg";

        var svg = BuildDefaultAvatarSvg(user.Id);
        var bytes = Encoding.UTF8.GetBytes(svg);
        return new RemoteStreamContent(new MemoryStream(bytes), fileName, "image/svg+xml");
    }

    private static string BuildDefaultAvatarSvg(Guid userId)
    {
        var seed = userId.ToByteArray();
        var hue = (seed[0] * 3 + seed[1]) % 360;
        var foregroundHue = (hue + 28 + seed[2] % 48) % 360;
        var background = $"hsl({hue} 46% 91%)";
        var foreground = $"hsl({foregroundHue} 62% 38%)";
        var accent = $"hsl({(foregroundHue + 36) % 360} 72% 54%)";

        var builder = new StringBuilder();
        builder.Append("""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 120" fill="none">
""");
        builder.AppendLine($"""<rect width="120" height="120" fill="{background}"/>""");
        builder.AppendLine($"""<circle cx="94" cy="26" r="18" fill="{accent}" opacity="0.18"/>""");
        builder.AppendLine($"""<circle cx="24" cy="100" r="22" fill="{foreground}" opacity="0.10"/>""");

        const int cell = 16;
        const int gap = 3;
        const int offset = 16;

        for (var row = 0; row < 5; row++)
        {
            var bits = seed[(row + 3) % seed.Length];
            for (var column = 0; column < 3; column++)
            {
                if (((bits >> column) & 1) == 0)
                {
                    continue;
                }

                var x = offset + column * (cell + gap);
                var mirroredColumn = 4 - column;
                var y = offset + row * (cell + gap);

                builder.AppendLine(
                    $"""<rect x="{x}" y="{y}" width="{cell}" height="{cell}" fill="{foreground}"/>"""
                );

                if (mirroredColumn != column)
                {
                    var mirroredX = offset + mirroredColumn * (cell + gap);
                    builder.AppendLine(
                        $"""<rect x="{mirroredX}" y="{y}" width="{cell}" height="{cell}" fill="{foreground}"/>"""
                    );
                }
            }
        }

        builder.Append("""
</svg>
""");

        return builder.ToString();
    }
}
