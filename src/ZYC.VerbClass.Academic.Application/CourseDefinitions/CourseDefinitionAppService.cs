using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.CourseDefinitions;

[Authorize]
public class CourseDefinitionAppService : ApplicationService, ICourseDefinitionAppService
{
    private readonly ICourseDefinitionRepository _courseDefinitionRepository;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly CourseDefinitionManager _courseDefinitionManager;

    public CourseDefinitionAppService(
        ICourseDefinitionRepository courseDefinitionRepository,
        ICourseOfferingRepository courseOfferingRepository,
        CourseDefinitionManager courseDefinitionManager)
    {
        _courseDefinitionRepository = courseDefinitionRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _courseDefinitionManager = courseDefinitionManager;
    }

    public async Task<CourseDefinitionListItemDto[]> GetListAsync()
    {
        var courseDefinitions = await _courseDefinitionRepository.GetListAsync();

        return courseDefinitions
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(AcademicApplicationDtoMapper.ToCourseDefinitionListItemDto)
            .ToArray();
    }

    public async Task<CourseDefinitionDetailDto> GetAsync(Guid courseDefinitionId)
    {
        var courseDefinition = await _courseDefinitionRepository.FindAsync(courseDefinitionId)
            ?? throw CreateCourseDefinitionNotFoundException();

        return AcademicApplicationDtoMapper.ToCourseDefinitionDetailDto(courseDefinition);
    }

    public async Task<CourseDefinitionOptionDto[]> GetActiveOptionsAsync()
    {
        var courseDefinitions = await _courseDefinitionRepository.GetListAsync();

        return courseDefinitions
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(AcademicApplicationDtoMapper.ToCourseDefinitionOptionDto)
            .ToArray();
    }

    public async Task<CourseDefinitionCommandResultDto> CreateAsync(CreateCourseDefinitionInput input)
    {
        ValidateInput(input);

        try
        {
            var courseDefinition = await _courseDefinitionManager.CreateAsync(
                input.Code,
                input.Name,
                input.Description
            );

            await _courseDefinitionRepository.InsertAsync(courseDefinition, true);

            return AcademicApplicationDtoMapper.ToCourseDefinitionCommandResultDto(courseDefinition);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseDefinitionCommandResultDto> UpdateAsync(
        Guid courseDefinitionId,
        UpdateCourseDefinitionInput input)
    {
        ValidateInput(input);

        try
        {
            var courseDefinition = await _courseDefinitionRepository.FindAsync(courseDefinitionId)
                ?? throw CreateCourseDefinitionNotFoundException();

            if (!string.Equals(courseDefinition.Code, input.Code, StringComparison.Ordinal))
            {
                await _courseDefinitionManager.ChangeCodeAsync(courseDefinition, input.Code);
            }

            courseDefinition.ChangeName(input.Name);
            courseDefinition.ChangeDescription(input.Description);

            await _courseDefinitionRepository.UpdateAsync(courseDefinition, true);

            return AcademicApplicationDtoMapper.ToCourseDefinitionCommandResultDto(courseDefinition);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseDefinitionCommandResultDto> ActivateAsync(Guid courseDefinitionId)
    {
        var courseDefinition = await _courseDefinitionRepository.FindAsync(courseDefinitionId)
            ?? throw CreateCourseDefinitionNotFoundException();

        courseDefinition.Activate();
        await _courseDefinitionRepository.UpdateAsync(courseDefinition, true);

        return AcademicApplicationDtoMapper.ToCourseDefinitionCommandResultDto(courseDefinition);
    }

    public async Task<CourseDefinitionCommandResultDto> DeactivateAsync(Guid courseDefinitionId)
    {
        var courseDefinition = await _courseDefinitionRepository.FindAsync(courseDefinitionId)
            ?? throw CreateCourseDefinitionNotFoundException();

        courseDefinition.Deactivate();
        await _courseDefinitionRepository.UpdateAsync(courseDefinition, true);

        return AcademicApplicationDtoMapper.ToCourseDefinitionCommandResultDto(courseDefinition);
    }

    public async Task<CourseDefinitionCommandResultDto> DeleteAsync(Guid courseDefinitionId)
    {
        var courseDefinition = await _courseDefinitionRepository.FindAsync(courseDefinitionId)
            ?? throw CreateCourseDefinitionNotFoundException();

        if (await _courseOfferingRepository.HasCourseDefinitionAsync(courseDefinition.Id))
        {
            throw new UserFriendlyException("Course definitions with offerings cannot be deleted. Deactivate it instead.");
        }

        await _courseDefinitionRepository.DeleteAsync(courseDefinition, true);

        return AcademicApplicationDtoMapper.ToCourseDefinitionCommandResultDto(courseDefinition);
    }

    private static void ValidateInput(CourseDefinitionInputBase input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course definition input is invalid.", validationErrors);
        }
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            CourseDefinitionErrorCodes.CodeAlreadyExists => "Course code already exists.",
            CourseDefinitionErrorCodes.DescriptionTooLong => "Description is too long.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Course definition operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            CourseDefinitionErrorCodes.CodeAlreadyExists => nameof(CourseDefinitionInputBase.Code),
            CourseDefinitionErrorCodes.DescriptionTooLong => nameof(CourseDefinitionInputBase.Description),
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
