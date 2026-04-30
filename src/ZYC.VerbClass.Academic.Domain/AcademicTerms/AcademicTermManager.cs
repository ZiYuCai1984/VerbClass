using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.AcademicTerms;

public class AcademicTermManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IAcademicTermRepository _academicTermRepository;

    public AcademicTermManager(
        ICurrentTenant currentTenant,
        IAcademicTermRepository academicTermRepository)
    {
        _currentTenant = currentTenant;
        _academicTermRepository = academicTermRepository;
    }

    public virtual async Task<AcademicTerm> CreateAsync(
        int academicYear,
        string code,
        string name,
        DateTime startDate,
        DateTime endDate,
        IEnumerable<TermPeriodDefinition> periods,
        CancellationToken cancellationToken = default)
    {
        var normalizedAcademicYear = AcademicTerm.NormalizeAcademicYear(academicYear);
        var normalizedCode = AcademicTerm.NormalizeCode(code);

        await ValidateCodeAsync(
            normalizedAcademicYear,
            normalizedCode,
            null,
            cancellationToken
        );

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("Academic term must be created within a tenant context.");

        return new AcademicTerm(
            GuidGenerator.Create(),
            tenantId,
            normalizedAcademicYear,
            normalizedCode,
            name,
            startDate,
            endDate,
            periods
        );
    }

    public virtual async Task ChangeIdentityAsync(
        AcademicTerm academicTerm,
        int academicYear,
        string code,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(academicTerm, nameof(academicTerm));

        var normalizedAcademicYear = AcademicTerm.NormalizeAcademicYear(academicYear);
        var normalizedCode = AcademicTerm.NormalizeCode(code);

        await ValidateCodeAsync(
            normalizedAcademicYear,
            normalizedCode,
            academicTerm.Id,
            cancellationToken
        );

        academicTerm.ChangeIdentity(normalizedAcademicYear, normalizedCode);
    }

    protected virtual async Task ValidateCodeAsync(
        int academicYear,
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _academicTermRepository.IsCodeExistsAsync(
            academicYear,
            code,
            excludeId,
            cancellationToken
        );

        if (exists)
        {
            throw new BusinessException(AcademicTermErrorCodes.CodeAlreadyExists)
                .WithData(nameof(AcademicTerm.AcademicYear), academicYear)
                .WithData(nameof(AcademicTerm.Code), code);
        }
    }
}
