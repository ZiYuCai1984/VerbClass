using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseDefinitions;

[Audited]
public class CourseDefinition : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    protected CourseDefinition()
    {
    }

    public CourseDefinition(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? description = null) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        ChangeCode(code);
        ChangeName(name);
        ChangeDescription(description);
        IsActive = true;
    }

    public Guid? TenantId { get; protected set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void ChangeCode(string code)
    {
        Code = NormalizeCode(code);
    }

    public void ChangeName(string name)
    {
        Name = NormalizeName(name);
    }

    public void ChangeDescription(string? description)
    {
        Description = NormalizeDescription(description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void EnsureActiveForOffering()
    {
        if (!IsActive)
        {
            throw new BusinessException(CourseDefinitionErrorCodes.CourseDefinitionInactive)
                .WithData(nameof(Id), Id)
                .WithData(nameof(Code), Code);
        }
    }

    internal static string NormalizeCode(string code)
    {
        return Check.NotNullOrWhiteSpace(
                code,
                nameof(code),
                CourseDefinitionConsts.MaxCodeLength
            )
            .Trim();
    }

    internal static string NormalizeName(string name)
    {
        return Check.NotNullOrWhiteSpace(
                name,
                nameof(name),
                CourseDefinitionConsts.MaxNameLength
            )
            .Trim();
    }

    private static string NormalizeDescription(string? description)
    {
        var normalizedDescription = description?.Trim() ?? string.Empty;
        if (normalizedDescription.Length > CourseDefinitionConsts.MaxDescriptionLength)
        {
            throw new BusinessException(CourseDefinitionErrorCodes.DescriptionTooLong)
                .WithData(nameof(Description), normalizedDescription.Length);
        }

        return normalizedDescription;
    }
}
