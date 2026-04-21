using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace ZYC.VerbClass.Domain.DepartmentAssignments;

public class DepartmentAssignment : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid UserId { get; private set; }

    public Guid DepartmentId { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTime EffectiveFrom { get; private set; }

    public DateTime? EffectiveTo { get; private set; }

    protected DepartmentAssignment()
    {
    }

    public DepartmentAssignment(
        Guid id,
        Guid tenantId,
        Guid userId,
        Guid departmentId,
        bool isPrimary,
        DateTime effectiveFrom,
        DateTime? effectiveTo = null) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new AbpException("TenantId can not be empty.");
        }

        TenantId = tenantId;
        UserId = userId;
        DepartmentId = departmentId;
        IsPrimary = isPrimary;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;

        ValidatePeriod();
    }

    public void ChangeDepartment(Guid departmentId)
    {
        DepartmentId = departmentId;
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    public void ChangePeriod(DateTime effectiveFrom, DateTime? effectiveTo)
    {
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        ValidatePeriod();
    }

    public void Close(DateTime effectiveTo)
    {
        EffectiveTo = effectiveTo;
        ValidatePeriod();
    }

    private void ValidatePeriod()
    {
        if (EffectiveTo.HasValue && EffectiveFrom > EffectiveTo.Value)
        {
            throw new BusinessException("DepartmentAssignment.Period.Invalid");
        }
    }
}
