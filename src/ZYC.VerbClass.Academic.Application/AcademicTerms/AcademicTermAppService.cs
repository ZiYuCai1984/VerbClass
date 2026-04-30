using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;
using AcademicSettingNames = ZYC.VerbClass.Academic.Domain.AcademicSettings;

namespace ZYC.VerbClass.Academic.Application.AcademicTerms;

[Authorize]
public class AcademicTermAppService : ApplicationService, IAcademicTermAppService
{
    private readonly IAcademicTermRepository _academicTermRepository;
    private readonly AcademicTermManager _academicTermManager;
    private readonly IAcademicTimeTemplateRepository _academicTimeTemplateRepository;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly ISettingManager _settingManager;

    public AcademicTermAppService(
        IAcademicTermRepository academicTermRepository,
        AcademicTermManager academicTermManager,
        IAcademicTimeTemplateRepository academicTimeTemplateRepository,
        ICourseOfferingRepository courseOfferingRepository,
        ISettingManager settingManager)
    {
        _academicTermRepository = academicTermRepository;
        _academicTermManager = academicTermManager;
        _academicTimeTemplateRepository = academicTimeTemplateRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _settingManager = settingManager;
    }

    public async Task<AcademicTermListItemDto[]> GetListAsync()
    {
        var terms = await _academicTermRepository.GetListAsync();

        return terms
            .OrderByDescending(x => x.AcademicYear)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Select(term => new AcademicTermListItemDto
            {
                Id = term.Id,
                AcademicYear = term.AcademicYear,
                Code = term.Code,
                Name = term.Name,
                StartDate = DateOnly.FromDateTime(term.StartDate),
                EndDate = DateOnly.FromDateTime(term.EndDate),
                IsLocked = term.IsLocked,
                PeriodCount = term.Periods.Count
            })
            .ToArray();
    }

    public async Task<AcademicTermDetailDto> GetAsync(Guid termId)
    {
        var term = await _academicTermRepository.FindAsync(termId)
            ?? throw CreateTermNotFoundException();

        return AcademicApplicationDtoMapper.ToAcademicTermDetailDto(term);
    }

    public async Task<AcademicTermCommandResultDto> CreateAsync(CreateAcademicTermInput input)
    {
        ValidateCreateInput(input);

        var template = await _academicTimeTemplateRepository.FindAsync(input.TimeTemplateId!.Value)
            ?? throw CreateTemplateValidationException();

        try
        {
            var term = await _academicTermManager.CreateAsync(
                input.AcademicYear,
                input.Code,
                input.Name,
                input.StartDate.ToDateTime(TimeOnly.MinValue),
                input.EndDate.ToDateTime(TimeOnly.MinValue),
                template.Periods.Select(period => TermPeriodDefinition.Create(
                    period.PeriodNo,
                    period.Label,
                    period.StartTime,
                    period.EndTime))
            );

            await _academicTermRepository.InsertAsync(term, true);

            return AcademicApplicationDtoMapper.ToAcademicTermCommandResultDto(term);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<AcademicTermCommandResultDto> UpdateAsync(Guid termId, UpdateAcademicTermInput input)
    {
        ValidateUpdateInput(input);

        try
        {
            var term = await _academicTermRepository.FindAsync(termId)
                ?? throw CreateTermNotFoundException();

            if (term.AcademicYear != input.AcademicYear ||
                !string.Equals(term.Code, input.Code, StringComparison.Ordinal))
            {
                await _academicTermManager.ChangeIdentityAsync(term, input.AcademicYear, input.Code);
            }

            term.ChangeName(input.Name);
            term.ChangeDateRange(
                input.StartDate.ToDateTime(TimeOnly.MinValue),
                input.EndDate.ToDateTime(TimeOnly.MinValue)
            );

            await _academicTermRepository.UpdateAsync(term, true);

            return AcademicApplicationDtoMapper.ToAcademicTermCommandResultDto(term);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<AcademicTermCommandResultDto> LockAsync(Guid termId)
    {
        try
        {
            var term = await _academicTermRepository.FindAsync(termId)
                ?? throw CreateTermNotFoundException();

            term.Lock();
            await _academicTermRepository.UpdateAsync(term, true);

            return AcademicApplicationDtoMapper.ToAcademicTermCommandResultDto(term);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<AcademicTermCommandResultDto> DeleteAsync(Guid termId)
    {
        var term = await _academicTermRepository.FindAsync(termId)
            ?? throw CreateTermNotFoundException();

        await EnsureTermIsNotCurrentAsync(termId);

        if (await _courseOfferingRepository.HasAcademicTermAsync(term.Id))
        {
            throw new UserFriendlyException("Academic terms with offerings cannot be deleted.");
        }

        await _academicTermRepository.DeleteAsync(term, true);

        return AcademicApplicationDtoMapper.ToAcademicTermCommandResultDto(term);
    }

    private async Task EnsureTermIsNotCurrentAsync(Guid termId)
    {
        var currentTermValue = await _settingManager.GetOrNullForCurrentTenantAsync(AcademicSettingNames.CurrentTermId);
        if (string.IsNullOrWhiteSpace(currentTermValue))
        {
            return;
        }

        if (!Guid.TryParse(currentTermValue, out var currentTermId) || currentTermId == Guid.Empty)
        {
            throw new UserFriendlyException("Current academic term setting is invalid.");
        }

        if (currentTermId == termId)
        {
            throw new UserFriendlyException("Unset the current academic term before deleting it.");
        }
    }

    private static void ValidateCreateInput(CreateAcademicTermInput input)
    {
        var validationErrors = ValidateTermInput(input);

        if (!input.TimeTemplateId.HasValue || input.TimeTemplateId.Value == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "Time template is required.",
                [nameof(input.TimeTemplateId)]
            ));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Academic term input is invalid.", validationErrors);
        }
    }

    private static void ValidateUpdateInput(UpdateAcademicTermInput input)
    {
        var validationErrors = ValidateTermInput(input);

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Academic term input is invalid.", validationErrors);
        }
    }

    private static List<ValidationResult> ValidateTermInput(object input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        switch (input)
        {
            case CreateAcademicTermInput createInput:
                ValidateDates(createInput.StartDate, createInput.EndDate, validationErrors);
                break;
            case UpdateAcademicTermInput updateInput:
                ValidateDates(updateInput.StartDate, updateInput.EndDate, validationErrors);
                break;
        }

        return validationErrors;
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate, List<ValidationResult> validationErrors)
    {
        if (startDate == default)
        {
            validationErrors.Add(new ValidationResult(
                "Start date is required.",
                [nameof(CreateAcademicTermInput.StartDate)]
            ));
        }

        if (endDate == default)
        {
            validationErrors.Add(new ValidationResult(
                "End date is required.",
                [nameof(CreateAcademicTermInput.EndDate)]
            ));
        }

        if (startDate != default && endDate != default && startDate > endDate)
        {
            validationErrors.Add(new ValidationResult(
                "End date must be later than start date.",
                [nameof(CreateAcademicTermInput.EndDate)]
            ));
        }
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            AcademicTermErrorCodes.CodeAlreadyExists => "Term code already exists for the academic year.",
            AcademicTermErrorCodes.AcademicYearOutOfRange => "Academic year is out of range.",
            AcademicTermErrorCodes.InvalidDateRange => "End date must be later than start date.",
            AcademicTermErrorCodes.TermLocked => "Locked terms cannot be modified.",
            AcademicTermErrorCodes.TermAlreadyLocked => "This term is already locked.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Academic term operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            AcademicTermErrorCodes.CodeAlreadyExists => nameof(UpdateAcademicTermInput.Code),
            AcademicTermErrorCodes.AcademicYearOutOfRange => nameof(UpdateAcademicTermInput.AcademicYear),
            AcademicTermErrorCodes.InvalidDateRange => nameof(UpdateAcademicTermInput.EndDate),
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

    private static UserFriendlyException CreateTermNotFoundException()
    {
        return new UserFriendlyException("Academic term was not found.");
    }

    private static AbpValidationException CreateTemplateValidationException()
    {
        return new AbpValidationException(
            "Time template was not found.",
            [new ValidationResult("Time template was not found.", [nameof(CreateAcademicTermInput.TimeTemplateId)])]
        );
    }
}
