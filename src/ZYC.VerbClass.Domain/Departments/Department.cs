using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Domain.Departments;

public class Department : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    protected Department()
    {
    }

    public Department(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        Guid? parentDepartmentId = null,
        string? shortName = null,
        int sort = 0,
        bool canAssignUsers = true,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        SetCode(code);
        SetName(name);
        SetShortName(shortName);
        SetSort(sort);
        SetCanAssignUsers(canAssignUsers);
        SetEffectivePeriod(effectiveFrom, effectiveTo);

        ParentDepartmentId = parentDepartmentId;
        IsActive = true;

        //!WARNING Path is calculated uniformly by the Manager
        Path = "/";
    }

    public Guid? TenantId { get; protected set; }

    public Guid? ParentDepartmentId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? ShortName { get; private set; }

    /// <summary>
    ///     !WARNING Redundant paths used for tree queries, such as: /root/dev/backend/
    /// </summary>
    public string Path { get; private set; } = string.Empty;

    public int Sort { get; private set; }

    public bool IsActive { get; private set; }

    public bool CanAssignUsers { get; private set; }

    public DateTime? EffectiveFrom { get; private set; }

    public DateTime? EffectiveTo { get; private set; }

    public void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            DepartmentConsts.MaxCodeLength
        );
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            DepartmentConsts.MaxNameLength
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
            DepartmentConsts.MaxShortNameLength
        );
    }

    public void SetSort(int sort)
    {
        Sort = sort;
    }

    public void SetCanAssignUsers(bool canAssignUsers)
    {
        CanAssignUsers = canAssignUsers;
    }

    public void Enable()
    {
        IsActive = true;
    }

    public void Disable()
    {
        IsActive = false;
    }

    public void SetEffectivePeriod(DateTime? effectiveFrom, DateTime? effectiveTo)
    {
        if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveFrom > effectiveTo)
        {
            throw new BusinessException(DepartmentErrorCodes.InvalidEffectivePeriod);
        }

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public void ChangeParent(Guid? parentDepartmentId)
    {
        if (parentDepartmentId.HasValue && parentDepartmentId.Value == Id)
        {
            throw new BusinessException(DepartmentErrorCodes.ParentCannotBeSelf);
        }

        ParentDepartmentId = parentDepartmentId;
    }

    public void SetPath(string path)
    {
        Path = Check.NotNullOrWhiteSpace(
            path,
            nameof(path),
            DepartmentConsts.MaxPathLength
        );
    }

    public bool IsCurrentlyEffective(DateTime now)
    {
        if (!IsActive)
        {
            return false;
        }

        if (EffectiveFrom.HasValue && now < EffectiveFrom.Value)
        {
            return false;
        }

        if (EffectiveTo.HasValue && now > EffectiveTo.Value)
        {
            return false;
        }

        return true;
    }
}
