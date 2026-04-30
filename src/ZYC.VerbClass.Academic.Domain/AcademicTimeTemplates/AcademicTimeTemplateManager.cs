using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;

public class AcademicTimeTemplateManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IAcademicTimeTemplateRepository _academicTimeTemplateRepository;

    public AcademicTimeTemplateManager(
        ICurrentTenant currentTenant,
        IAcademicTimeTemplateRepository academicTimeTemplateRepository)
    {
        _currentTenant = currentTenant;
        _academicTimeTemplateRepository = academicTimeTemplateRepository;
    }

    public virtual async Task<AcademicTimeTemplate> CreateAsync(
        string code,
        string name,
        IEnumerable<TimeTemplatePeriodDefinition> periods,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = AcademicTimeTemplate.NormalizeCode(code);

        await ValidateCodeAsync(normalizedCode, null, cancellationToken);

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("Academic time template must be created within a tenant context.");

        return new AcademicTimeTemplate(
            GuidGenerator.Create(),
            tenantId,
            normalizedCode,
            name,
            periods
        );
    }

    public virtual async Task ChangeCodeAsync(
        AcademicTimeTemplate academicTimeTemplate,
        string code,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(academicTimeTemplate, nameof(academicTimeTemplate));

        var normalizedCode = AcademicTimeTemplate.NormalizeCode(code);

        await ValidateCodeAsync(normalizedCode, academicTimeTemplate.Id, cancellationToken);

        academicTimeTemplate.ChangeCode(normalizedCode);
    }

    protected virtual async Task ValidateCodeAsync(
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _academicTimeTemplateRepository.IsCodeExistsAsync(
            code,
            excludeId,
            cancellationToken
        );

        if (exists)
        {
            throw new BusinessException(AcademicTimeTemplateErrorCodes.CodeAlreadyExists)
                .WithData(nameof(AcademicTimeTemplate.Code), code);
        }
    }
}
