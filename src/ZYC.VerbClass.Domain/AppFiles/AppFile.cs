using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace ZYC.VerbClass.Domain.AppFiles;

public class AppFile : AggregateRoot<Guid>, IMultiTenant
{
    protected AppFile()
    {
    }

    public AppFile(
        Guid id,
        Guid tenantId,
        string fileName,
        string contentType,
        long size,
        string blobName) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        if (size < 0)
        {
            throw new AbpException("Size can not be negative.");
        }

        TenantId = tenantId;
        FileName = Normalize(fileName, nameof(fileName), 256);
        ContentType = Normalize(contentType, nameof(contentType), 128);
        Size = size;
        BlobName = Normalize(blobName, nameof(blobName), 256);
    }

    public Guid? TenantId { get; protected set; }

    public string FileName { get; protected set; } = null!;

    public string ContentType { get; protected set; } = null!;

    public long Size { get; protected set; }

    public string BlobName { get; protected set; } = null!;

    private static string Normalize(string value, string parameterName, int maxLength)
    {
        Check.NotNullOrWhiteSpace(value, parameterName);
        return Check.Length(value.Trim(), parameterName, maxLength)!;
    }
}
