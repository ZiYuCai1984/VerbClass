using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.CourseOfferings;

[Authorize]
public class CourseOfferingAppService : ApplicationService, ICourseOfferingAppService
{
    private readonly IAcademicTermRepository _academicTermRepository;
    private readonly ICourseDefinitionRepository _courseDefinitionRepository;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly ICourseOfferingParticipantRepository _participantRepository;
    private readonly CourseOfferingManager _courseOfferingManager;

    public CourseOfferingAppService(
        IAcademicTermRepository academicTermRepository,
        ICourseDefinitionRepository courseDefinitionRepository,
        ICourseOfferingRepository courseOfferingRepository,
        ICourseOfferingParticipantRepository participantRepository,
        CourseOfferingManager courseOfferingManager)
    {
        _academicTermRepository = academicTermRepository;
        _courseDefinitionRepository = courseDefinitionRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _participantRepository = participantRepository;
        _courseOfferingManager = courseOfferingManager;
    }

    public async Task<CourseOfferingListItemDto[]> GetListAsync(Guid academicTermId)
    {
        await EnsureTermExistsAsync(academicTermId);

        var courseOfferings = await _courseOfferingRepository.GetListByTermAsync(academicTermId);

        return courseOfferings
            .OrderBy(x => x.OfferingCode, StringComparer.OrdinalIgnoreCase)
            .Select(AcademicApplicationDtoMapper.ToCourseOfferingListItemDto)
            .ToArray();
    }

    public async Task<CourseOfferingDetailDto> GetAsync(Guid courseOfferingId)
    {
        var courseOffering = await _courseOfferingRepository.FindAsync(courseOfferingId)
            ?? throw CreateCourseOfferingNotFoundException();

        var academicTerm = await _academicTermRepository.FindAsync(courseOffering.AcademicTermId)
            ?? throw CreateTermNotFoundException();

        return MapDetail(courseOffering, academicTerm);
    }

    public async Task<CourseOfferingCommandResultDto> CreateAsync(CreateCourseOfferingInput input)
    {
        ValidateCreateInput(input);

        var academicTerm = await _academicTermRepository.FindAsync(input.AcademicTermId!.Value)
            ?? throw CreateTermValidationException();

        var courseDefinition = await _courseDefinitionRepository.FindAsync(input.CourseDefinitionId!.Value)
            ?? throw CreateCourseDefinitionValidationException();

        try
        {
            var courseOffering = await _courseOfferingManager.CreateAsync(
                academicTerm,
                courseDefinition,
                input.OfferingCode,
                ToScheduleSlots(input.ScheduleSlots)
            );

            await _courseOfferingRepository.InsertAsync(courseOffering, true);

            return AcademicApplicationDtoMapper.ToCourseOfferingCommandResultDto(courseOffering);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseOfferingCommandResultDto> UpdateAsync(
        Guid courseOfferingId,
        UpdateCourseOfferingInput input)
    {
        ValidateUpdateInput(input);

        try
        {
            var courseOffering = await _courseOfferingRepository.FindAsync(courseOfferingId)
                ?? throw CreateCourseOfferingNotFoundException();

            var academicTerm = await _academicTermRepository.FindAsync(courseOffering.AcademicTermId)
                ?? throw CreateTermNotFoundException();

            if (!string.Equals(courseOffering.OfferingCode, input.OfferingCode, StringComparison.Ordinal))
            {
                await _courseOfferingManager.ChangeOfferingCodeAsync(courseOffering, input.OfferingCode);
            }

            courseOffering.ReplaceScheduleSlots(
                ToScheduleSlots(input.ScheduleSlots),
                academicTerm.Periods.Select(x => x.PeriodNo)
            );

            await _courseOfferingRepository.UpdateAsync(courseOffering, true);

            return AcademicApplicationDtoMapper.ToCourseOfferingCommandResultDto(courseOffering);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseOfferingCommandResultDto> DeleteAsync(Guid courseOfferingId)
    {
        var courseOffering = await _courseOfferingRepository.FindAsync(courseOfferingId)
            ?? throw CreateCourseOfferingNotFoundException();

        if (await _participantRepository.HasCourseOfferingAsync(courseOffering.Id))
        {
            throw new UserFriendlyException("Course offerings with participants cannot be deleted.");
        }

        await _courseOfferingRepository.DeleteAsync(courseOffering, true);

        return AcademicApplicationDtoMapper.ToCourseOfferingCommandResultDto(courseOffering);
    }

    private async Task EnsureTermExistsAsync(Guid academicTermId)
    {
        if (academicTermId == Guid.Empty)
        {
            throw CreateTermNotFoundException();
        }

        _ = await _academicTermRepository.FindAsync(academicTermId)
            ?? throw CreateTermNotFoundException();
    }

    private static CourseOfferingDetailDto MapDetail(CourseOffering courseOffering, AcademicTerm academicTerm)
    {
        return new CourseOfferingDetailDto
        {
            Id = courseOffering.Id,
            AcademicTermId = courseOffering.AcademicTermId,
            AcademicYear = academicTerm.AcademicYear,
            AcademicTermCode = academicTerm.Code,
            AcademicTermName = academicTerm.Name,
            CourseDefinitionId = courseOffering.CourseDefinitionId,
            OfferingCode = courseOffering.OfferingCode,
            CourseCodeSnapshot = courseOffering.CourseCodeSnapshot,
            CourseNameSnapshot = courseOffering.CourseNameSnapshot,
            ScheduleSlots = AcademicApplicationDtoMapper.ToAcademicScheduleSlotDtos(courseOffering.ScheduleSlots),
            TermPeriods = AcademicApplicationDtoMapper.ToAcademicPeriodDefinitionDtos(academicTerm.Periods)
        };
    }

    private static CourseOfferingScheduleSlot[] ToScheduleSlots(
        IEnumerable<AcademicScheduleSlotDto> scheduleSlots)
    {
        return scheduleSlots
            .Select(scheduleSlot => CourseOfferingScheduleSlot.Create(
                scheduleSlot.Weekday,
                scheduleSlot.PeriodNo))
            .ToArray();
    }

    private static void ValidateCreateInput(CreateCourseOfferingInput input)
    {
        var validationErrors = ValidateOfferingInput(input);

        if (!input.AcademicTermId.HasValue || input.AcademicTermId.Value == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "Academic term is required.",
                [nameof(input.AcademicTermId)]
            ));
        }

        if (!input.CourseDefinitionId.HasValue || input.CourseDefinitionId.Value == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "Course definition is required.",
                [nameof(input.CourseDefinitionId)]
            ));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course offering input is invalid.", validationErrors);
        }
    }

    private static void ValidateUpdateInput(UpdateCourseOfferingInput input)
    {
        var validationErrors = ValidateOfferingInput(input);

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course offering input is invalid.", validationErrors);
        }
    }

    private static List<ValidationResult> ValidateOfferingInput(CourseOfferingInputBase input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (input.ScheduleSlots is null)
        {
            validationErrors.Add(new ValidationResult(
                "Schedule slots are required.",
                [nameof(input.ScheduleSlots)]
            ));

            return validationErrors;
        }

        for (var i = 0; i < input.ScheduleSlots.Length; i++)
        {
            var scheduleSlot = input.ScheduleSlots[i];
            if (scheduleSlot is null)
            {
                validationErrors.Add(new ValidationResult(
                    "Schedule slot is required.",
                    [$"{nameof(input.ScheduleSlots)}[{i}]"]
                ));
                continue;
            }

            var scheduleSlotValidationResults = new List<ValidationResult>();
            Validator.TryValidateObject(
                scheduleSlot,
                new ValidationContext(scheduleSlot),
                scheduleSlotValidationResults,
                true
            );

            foreach (var validationResult in scheduleSlotValidationResults)
            {
                var members = validationResult.MemberNames.Any()
                    ? validationResult.MemberNames.Select(member => $"{nameof(input.ScheduleSlots)}[{i}].{member}")
                    : [$"{nameof(input.ScheduleSlots)}[{i}]"];

                validationErrors.Add(new ValidationResult(
                    validationResult.ErrorMessage ?? "Schedule slot is invalid.",
                    members
                ));
            }
        }

        return validationErrors;
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            CourseOfferingErrorCodes.OfferingCodeAlreadyExists => "Offering code already exists in this term.",
            CourseOfferingErrorCodes.InvalidWeekday => "Weekday is invalid.",
            CourseOfferingErrorCodes.InvalidPeriodNo => "Period number must be greater than zero.",
            CourseOfferingErrorCodes.UnknownTermPeriodNo => "Schedule period does not exist in the selected term.",
            CourseOfferingErrorCodes.DuplicateScheduleSlot => "Schedule slots must be unique.",
            CourseDefinitionErrorCodes.CourseDefinitionInactive => "Inactive courses cannot be opened.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Course offering operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            CourseOfferingErrorCodes.OfferingCodeAlreadyExists => nameof(CourseOfferingInputBase.OfferingCode),
            CourseOfferingErrorCodes.InvalidWeekday or
                CourseOfferingErrorCodes.InvalidPeriodNo or
                CourseOfferingErrorCodes.UnknownTermPeriodNo or
                CourseOfferingErrorCodes.DuplicateScheduleSlot => nameof(CourseOfferingInputBase.ScheduleSlots),
            CourseDefinitionErrorCodes.CourseDefinitionInactive => nameof(CreateCourseOfferingInput.CourseDefinitionId),
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

    private static UserFriendlyException CreateCourseOfferingNotFoundException()
    {
        return new UserFriendlyException("Course offering was not found.");
    }

    private static UserFriendlyException CreateTermNotFoundException()
    {
        return new UserFriendlyException("Academic term was not found.");
    }

    private static AbpValidationException CreateTermValidationException()
    {
        return new AbpValidationException(
            "Academic term was not found.",
            [new ValidationResult("Academic term was not found.", [nameof(CreateCourseOfferingInput.AcademicTermId)])]
        );
    }

    private static AbpValidationException CreateCourseDefinitionValidationException()
    {
        return new AbpValidationException(
            "Course definition was not found.",
            [new ValidationResult(
                "Course definition was not found.",
                [nameof(CreateCourseOfferingInput.CourseDefinitionId)]
            )]
        );
    }
}
