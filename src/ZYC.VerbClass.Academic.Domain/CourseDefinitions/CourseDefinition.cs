using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseDefinitions;

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
        string? shortName = null,
        string? description = null) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        SetShortName(shortName);
        SetDescription(description);
        IsActive = true;
    }

    public Guid? TenantId { get; protected set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? ShortName { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            CourseDefinitionConsts.MaxCodeLength
        );
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            CourseDefinitionConsts.MaxNameLength
        );
    }

    public void SetShortName(string? shortName)
    {
        if (shortName.IsNullOrWhiteSpace())
        {
            ShortName = null;
            return;
        }

        ShortName = Check.Length(
            shortName.Trim(),
            nameof(shortName),
            CourseDefinitionConsts.MaxShortNameLength
        );
    }

    public void SetDescription(string? description)
    {
        if (description.IsNullOrWhiteSpace())
        {
            Description = null;
            return;
        }

        Description = Check.Length(
            description.Trim(),
            nameof(description),
            CourseDefinitionConsts.MaxDescriptionLength
        );
    }

    public void Enable()
    {
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }
}
