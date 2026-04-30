using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseDefinitions;

public class CourseDefinitionManager : DomainService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICourseDefinitionRepository _courseDefinitionRepository;

    public CourseDefinitionManager(
        ICurrentTenant currentTenant,
        ICourseDefinitionRepository courseDefinitionRepository)
    {
        _currentTenant = currentTenant;
        _courseDefinitionRepository = courseDefinitionRepository;
    }

    public virtual async Task<CourseDefinition> CreateAsync(
        string code,
        string name,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = CourseDefinition.NormalizeCode(code);

        await ValidateCodeAsync(normalizedCode, null, cancellationToken);

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("Course definition must be created within a tenant context.");

        return new CourseDefinition(
            GuidGenerator.Create(),
            tenantId,
            normalizedCode,
            name,
            description
        );
    }

    public virtual async Task ChangeCodeAsync(
        CourseDefinition courseDefinition,
        string code,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(courseDefinition, nameof(courseDefinition));

        var normalizedCode = CourseDefinition.NormalizeCode(code);

        await ValidateCodeAsync(normalizedCode, courseDefinition.Id, cancellationToken);

        courseDefinition.ChangeCode(normalizedCode);
    }

    protected virtual async Task ValidateCodeAsync(
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await _courseDefinitionRepository.IsCodeExistsAsync(
            code,
            excludeId,
            cancellationToken
        );

        if (exists)
        {
            throw new BusinessException(CourseDefinitionErrorCodes.CodeAlreadyExists)
                .WithData(nameof(CourseDefinition.Code), code);
        }
    }
}
