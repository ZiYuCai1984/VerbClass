using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.AcademicTimeTemplates;

[Authorize]
public class AcademicTimeTemplateAppService : ApplicationService, IAcademicTimeTemplateAppService
{
    private readonly IAcademicTimeTemplateRepository _academicTimeTemplateRepository;
    private readonly AcademicTimeTemplateManager _academicTimeTemplateManager;

    public AcademicTimeTemplateAppService(
        IAcademicTimeTemplateRepository academicTimeTemplateRepository,
        AcademicTimeTemplateManager academicTimeTemplateManager)
    {
        _academicTimeTemplateRepository = academicTimeTemplateRepository;
        _academicTimeTemplateManager = academicTimeTemplateManager;
    }

    public async Task<AcademicTimeTemplateListItemDto[]> GetListAsync()
    {
        var templates = await _academicTimeTemplateRepository.GetListAsync();

        return templates
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Select(template => new AcademicTimeTemplateListItemDto
            {
                Id = template.Id,
                Code = template.Code,
                Name = template.Name,
                PeriodCount = template.Periods.Count
            })
            .ToArray();
    }

    public async Task<AcademicTimeTemplateDetailDto> GetAsync(Guid templateId)
    {
        var template = await _academicTimeTemplateRepository.FindAsync(templateId)
            ?? throw CreateTemplateNotFoundException();

        return AcademicApplicationDtoMapper.ToAcademicTimeTemplateDetailDto(template);
    }

    public async Task<AcademicTimeTemplateOptionDto[]> GetOptionsAsync()
    {
        var templates = await _academicTimeTemplateRepository.GetListAsync();

        return templates
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Select(AcademicApplicationDtoMapper.ToAcademicTimeTemplateOptionDto)
            .ToArray();
    }

    public async Task<AcademicTimeTemplateCommandResultDto> CreateAsync(CreateAcademicTimeTemplateInput input)
    {
        ValidateInput(input);

        try
        {
            var template = await _academicTimeTemplateManager.CreateAsync(
                input.Code,
                input.Name,
                ToPeriodDefinitions(input.Periods)
            );

            await _academicTimeTemplateRepository.InsertAsync(template, true);

            return AcademicApplicationDtoMapper.ToAcademicTimeTemplateCommandResultDto(template);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<AcademicTimeTemplateCommandResultDto> UpdateAsync(
        Guid templateId,
        UpdateAcademicTimeTemplateInput input)
    {
        ValidateInput(input);

        try
        {
            var template = await _academicTimeTemplateRepository.FindAsync(templateId)
                ?? throw CreateTemplateNotFoundException();

            if (!string.Equals(template.Code, input.Code, StringComparison.Ordinal))
            {
                await _academicTimeTemplateManager.ChangeCodeAsync(template, input.Code);
            }

            template.ChangeName(input.Name);
            template.ReplacePeriods(ToPeriodDefinitions(input.Periods));

            await _academicTimeTemplateRepository.UpdateAsync(template, true);

            return AcademicApplicationDtoMapper.ToAcademicTimeTemplateCommandResultDto(template);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<AcademicTimeTemplateCommandResultDto> DeleteAsync(Guid templateId)
    {
        var template = await _academicTimeTemplateRepository.FindAsync(templateId)
            ?? throw CreateTemplateNotFoundException();

        await _academicTimeTemplateRepository.DeleteAsync(template, true);

        return AcademicApplicationDtoMapper.ToAcademicTimeTemplateCommandResultDto(template);
    }

    private static TimeTemplatePeriodDefinition[] ToPeriodDefinitions(
        IEnumerable<AcademicPeriodDefinitionDto> periods)
    {
        return periods
            .Select(period => TimeTemplatePeriodDefinition.Create(
                period.PeriodNo,
                period.Label,
                period.StartTime,
                period.EndTime))
            .ToArray();
    }

    private static void ValidateInput(AcademicTimeTemplateInputBase input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        var periods = input.Periods ?? [];

        if (periods.Length == 0)
        {
            validationErrors.Add(new ValidationResult(
                "At least one period is required.",
                [nameof(input.Periods)]
            ));
        }

        for (var i = 0; i < periods.Length; i++)
        {
            var period = periods[i];
            if (period is null)
            {
                validationErrors.Add(new ValidationResult(
                    "Period is required.",
                    [$"{nameof(input.Periods)}[{i}]"]
                ));
                continue;
            }

            var periodValidationResults = new List<ValidationResult>();
            Validator.TryValidateObject(period, new ValidationContext(period), periodValidationResults, true);

            foreach (var periodValidationResult in periodValidationResults)
            {
                var members = periodValidationResult.MemberNames.Any()
                    ? periodValidationResult.MemberNames.Select(member => $"{nameof(input.Periods)}[{i}].{member}")
                    : [$"{nameof(input.Periods)}[{i}]"];

                validationErrors.Add(new ValidationResult(
                    periodValidationResult.ErrorMessage ?? "Period is invalid.",
                    members
                ));
            }
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Academic time template input is invalid.", validationErrors);
        }
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            AcademicTimeTemplateErrorCodes.CodeAlreadyExists => "Template code already exists.",
            AcademicTimeTemplateErrorCodes.PeriodsCannotBeEmpty => "At least one period is required.",
            AcademicTimeTemplateErrorCodes.DuplicatePeriodNo => "Period numbers must be unique.",
            AcademicTimeTemplateErrorCodes.InvalidPeriodChronology => "Periods must not overlap.",
            AcademicTimeTemplateErrorCodes.InvalidPeriodNo => "Period number must be greater than zero.",
            AcademicTimeTemplateErrorCodes.InvalidPeriodTimeRange => "End time must be later than start time.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Academic time template operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            AcademicTimeTemplateErrorCodes.CodeAlreadyExists => nameof(AcademicTimeTemplateInputBase.Code),
            AcademicTimeTemplateErrorCodes.PeriodsCannotBeEmpty or
                AcademicTimeTemplateErrorCodes.DuplicatePeriodNo or
                AcademicTimeTemplateErrorCodes.InvalidPeriodChronology or
                AcademicTimeTemplateErrorCodes.InvalidPeriodNo or
                AcademicTimeTemplateErrorCodes.InvalidPeriodTimeRange => nameof(AcademicTimeTemplateInputBase.Periods),
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

    private static UserFriendlyException CreateTemplateNotFoundException()
    {
        return new UserFriendlyException("Time template was not found.");
    }
}
