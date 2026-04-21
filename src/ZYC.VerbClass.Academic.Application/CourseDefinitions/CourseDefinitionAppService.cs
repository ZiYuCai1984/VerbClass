using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.CourseDefinitions;

public class CourseDefinitionAppService : ApplicationService, ICourseDefinitionAppService
{
    private readonly CourseDefinitionManager _courseDefinitionManager;
    private readonly IRepository<CourseDefinition, Guid> _courseDefinitionRepository;
    private readonly IRepository<CourseOffering, Guid> _courseOfferingRepository;

    public CourseDefinitionAppService(
        CourseDefinitionManager courseDefinitionManager,
        IRepository<CourseDefinition, Guid> courseDefinitionRepository,
        IRepository<CourseOffering, Guid> courseOfferingRepository)
    {
        _courseDefinitionManager = courseDefinitionManager;
        _courseDefinitionRepository = courseDefinitionRepository;
        _courseOfferingRepository = courseOfferingRepository;
    }

    public async Task<CourseDefinitionListItemDto[]> GetListAsync()
    {
        var courseDefinitions = await _courseDefinitionRepository.GetListAsync();

        return courseDefinitions
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Name)
            .Select(x => new CourseDefinitionListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                ShortName = x.ShortName,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToArray();
    }

    public async Task<CourseDefinitionCommandResultDto> CreateAsync(CreateCourseDefinitionInput input)
    {
        try
        {
            var courseDefinition = await _courseDefinitionManager.CreateAsync(
                input.Code,
                input.Name,
                input.ShortName,
                input.Description
            );

            if (!input.IsActive)
            {
                courseDefinition.Disable();
            }

            await _courseDefinitionRepository.InsertAsync(courseDefinition, true);

            return MapCommandResult(courseDefinition);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseDefinitionCommandResultDto> UpdateAsync(Guid id, UpdateCourseDefinitionInput input)
    {
        try
        {
            var courseDefinition = await _courseDefinitionRepository.FindAsync(id)
                ?? throw CreateCourseDefinitionNotFoundException();

            if (!string.Equals(courseDefinition.Code, input.Code, StringComparison.Ordinal))
            {
                await _courseDefinitionManager.ChangeCodeAsync(courseDefinition, input.Code);
            }

            courseDefinition.SetName(input.Name);
            courseDefinition.SetShortName(input.ShortName);
            courseDefinition.SetDescription(input.Description);

            if (input.IsActive)
            {
                courseDefinition.Enable();
            }
            else
            {
                courseDefinition.Disable();
            }

            await _courseDefinitionRepository.UpdateAsync(courseDefinition, true);

            return MapCommandResult(courseDefinition);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseDefinitionCommandResultDto> DeleteAsync(Guid id)
    {
        var courseDefinition = await _courseDefinitionRepository.FindAsync(id)
            ?? throw CreateCourseDefinitionNotFoundException();

        var offerings = await _courseOfferingRepository.GetListAsync(x => x.CourseDefinitionId == id);
        if (offerings.Count > 0)
        {
            throw new UserFriendlyException("Delete course offerings before deleting this course definition.");
        }

        await _courseDefinitionRepository.DeleteAsync(courseDefinition, true);

        return MapCommandResult(courseDefinition);
    }

    private static CourseDefinitionCommandResultDto MapCommandResult(CourseDefinition courseDefinition)
    {
        return new CourseDefinitionCommandResultDto
        {
            Id = courseDefinition.Id,
            Code = courseDefinition.Code,
            Name = courseDefinition.Name,
            IsActive = courseDefinition.IsActive
        };
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            AcademicErrorCodes.CourseDefinitionCodeAlreadyExists => "Course definition code already exists.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Course definition operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            AcademicErrorCodes.CourseDefinitionCodeAlreadyExists => nameof(CourseDefinitionInputBase.Code),
            _ => null
        };

        if (fieldName is null)
        {
            return new UserFriendlyException(message, innerException: ex);
        }

        return new AbpValidationException(
            message,
            [new ValidationResult(message, [fieldName])]
        );
    }

    private static UserFriendlyException CreateCourseDefinitionNotFoundException()
    {
        return new UserFriendlyException("Course definition was not found.");
    }
}
