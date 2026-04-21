using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseDefinitions;

public class CourseDefinitionManager : DomainService
{
    private readonly IRepository<CourseDefinition, Guid> _courseDefinitionRepository;
    private readonly ICurrentTenant _currentTenant;

    public CourseDefinitionManager(
        IRepository<CourseDefinition, Guid> courseDefinitionRepository,
        ICurrentTenant currentTenant)
    {
        _courseDefinitionRepository = courseDefinitionRepository;
        _currentTenant = currentTenant;
    }

    public virtual async Task<CourseDefinition> CreateAsync(
        string code,
        string name,
        string? shortName = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        await ValidateCodeAsync(code, null, cancellationToken);

        var tenantId = _currentTenant.Id
            ?? throw new AbpException("CourseDefinition must be created within a tenant context.");

        return new CourseDefinition(
            GuidGenerator.Create(),
            tenantId,
            code,
            name,
            shortName,
            description
        );
    }

    public virtual async Task ChangeCodeAsync(
        CourseDefinition courseDefinition,
        string newCode,
        CancellationToken cancellationToken = default)
    {
        await ValidateCodeAsync(newCode, courseDefinition.Id, cancellationToken);
        courseDefinition.SetCode(newCode);
    }

    protected virtual async Task ValidateCodeAsync(
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var normalizedCode = code.Trim();

        var existingItems = await _courseDefinitionRepository.GetListAsync(
            x => x.Code == normalizedCode && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken: cancellationToken
        );

        if (existingItems.Count > 0)
        {
            throw new BusinessException(AcademicErrorCodes.CourseDefinitionCodeAlreadyExists)
                .WithData("Code", normalizedCode);
        }
    }
}
