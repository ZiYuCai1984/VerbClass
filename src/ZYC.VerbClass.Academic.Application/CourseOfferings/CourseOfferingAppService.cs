using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Memberships;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.CourseOfferings;

public class CourseOfferingAppService : ApplicationService, ICourseOfferingAppService
{
    private readonly CourseOfferingManager _courseOfferingManager;
    private readonly IRepository<CourseOffering, Guid> _courseOfferingRepository;
    private readonly IRepository<CourseDefinition, Guid> _courseDefinitionRepository;
    private readonly IRepository<CourseMembership, Guid> _courseMembershipRepository;

    public CourseOfferingAppService(
        CourseOfferingManager courseOfferingManager,
        IRepository<CourseOffering, Guid> courseOfferingRepository,
        IRepository<CourseDefinition, Guid> courseDefinitionRepository,
        IRepository<CourseMembership, Guid> courseMembershipRepository)
    {
        _courseOfferingManager = courseOfferingManager;
        _courseOfferingRepository = courseOfferingRepository;
        _courseDefinitionRepository = courseDefinitionRepository;
        _courseMembershipRepository = courseMembershipRepository;
    }

    public async Task<CourseOfferingListItemDto[]> GetListAsync()
    {
        var courseOfferings = await _courseOfferingRepository.GetListAsync();
        var courseDefinitionsById = await GetCourseDefinitionsByIdAsync(
            courseOfferings.Select(x => x.CourseDefinitionId)
        );

        return courseOfferings
            .OrderByDescending(x => x.AcademicYear)
            .ThenBy(x => x.TermName)
            .ThenBy(x => x.ScheduleDayOfWeek.HasValue ? 0 : 1)
            .ThenBy(x => x.ScheduleDayOfWeek)
            .ThenBy(x => x.ScheduleStartTime.HasValue ? 0 : 1)
            .ThenBy(x => x.ScheduleStartTime)
            .ThenBy(x => x.Id)
            .Select(x => MapListItem(
                x,
                courseDefinitionsById.GetValueOrDefault(x.CourseDefinitionId)
            ))
            .ToArray();
    }

    public async Task<CourseOfferingCommandResultDto> CreateAsync(CreateCourseOfferingInput input)
    {
        ValidateInput(input);

        try
        {
            var courseOffering = await _courseOfferingManager.CreateAsync(
                input.CourseDefinitionId,
                new AcademicTerm(input.AcademicYear, input.TermName),
                input.ScheduleDayOfWeek,
                input.ScheduleStartTime,
                input.ScheduleEndTime,
                input.ScheduleLocation,
                new EnrollmentPolicy(input.EnrollmentStartsAt, input.EnrollmentEndsAt)
            );

            await _courseOfferingRepository.InsertAsync(courseOffering, true);

            return MapCommandResult(courseOffering);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseOfferingCommandResultDto> UpdateAsync(Guid id, UpdateCourseOfferingInput input)
    {
        ValidateInput(input);

        try
        {
            var courseOffering = await _courseOfferingRepository.FindAsync(id)
                ?? throw CreateCourseOfferingNotFoundException();

            if (courseOffering.CourseDefinitionId != input.CourseDefinitionId)
            {
                throw new UserFriendlyException("Changing the course definition of an existing course offering is not supported.");
            }

            courseOffering.SetTerm(new AcademicTerm(input.AcademicYear, input.TermName));
            courseOffering.SetSchedule(
                input.ScheduleDayOfWeek,
                input.ScheduleStartTime,
                input.ScheduleEndTime,
                input.ScheduleLocation
            );
            courseOffering.SetEnrollmentPolicy(
                new EnrollmentPolicy(input.EnrollmentStartsAt, input.EnrollmentEndsAt)
            );

            await _courseOfferingRepository.UpdateAsync(courseOffering, true);

            return MapCommandResult(courseOffering);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseOfferingCommandResultDto> DeleteAsync(Guid id)
    {
        var courseOffering = await _courseOfferingRepository.FindAsync(id)
            ?? throw CreateCourseOfferingNotFoundException();

        var memberships = await _courseMembershipRepository.GetListAsync(x => x.CourseOfferingId == id);
        if (memberships.Count > 0)
        {
            throw new UserFriendlyException("Remove course memberships before deleting this course offering.");
        }

        await _courseOfferingRepository.DeleteAsync(courseOffering, true);

        return MapCommandResult(courseOffering);
    }

    public async Task<CourseOfferingCommandResultDto> LockAsync(Guid id)
    {
        var courseOffering = await _courseOfferingRepository.FindAsync(id)
            ?? throw CreateCourseOfferingNotFoundException();

        courseOffering.Lock();
        await _courseOfferingRepository.UpdateAsync(courseOffering, true);

        return MapCommandResult(courseOffering);
    }

    public async Task<CourseOfferingCommandResultDto> UnlockAsync(Guid id)
    {
        var courseOffering = await _courseOfferingRepository.FindAsync(id)
            ?? throw CreateCourseOfferingNotFoundException();

        courseOffering.Unlock();
        await _courseOfferingRepository.UpdateAsync(courseOffering, true);

        return MapCommandResult(courseOffering);
    }

    private async Task<IReadOnlyDictionary<Guid, CourseDefinition>> GetCourseDefinitionsByIdAsync(
        IEnumerable<Guid> courseDefinitionIds)
    {
        var normalizedIds = courseDefinitionIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedIds.Length == 0)
        {
            return new Dictionary<Guid, CourseDefinition>();
        }

        var courseDefinitions = await _courseDefinitionRepository.GetListAsync(
            x => normalizedIds.Contains(x.Id)
        );

        return courseDefinitions.ToDictionary(x => x.Id);
    }

    private static CourseOfferingListItemDto MapListItem(
        CourseOffering courseOffering,
        CourseDefinition? courseDefinition)
    {
        return new CourseOfferingListItemDto
        {
            Id = courseOffering.Id,
            CourseDefinitionId = courseOffering.CourseDefinitionId,
            CourseDefinitionCode = courseDefinition?.Code ?? string.Empty,
            CourseDefinitionName = courseDefinition?.Name ?? string.Empty,
            AcademicYear = courseOffering.AcademicYear,
            TermName = courseOffering.TermName,
            ScheduleDayOfWeek = courseOffering.ScheduleDayOfWeek,
            ScheduleStartTime = courseOffering.ScheduleStartTime,
            ScheduleEndTime = courseOffering.ScheduleEndTime,
            ScheduleLocation = courseOffering.ScheduleLocation,
            EnrollmentStartsAt = courseOffering.EnrollmentStartsAt,
            EnrollmentEndsAt = courseOffering.EnrollmentEndsAt,
            Status = courseOffering.Status,
            IsLocked = courseOffering.IsLocked
        };
    }

    private static CourseOfferingCommandResultDto MapCommandResult(CourseOffering courseOffering)
    {
        return new CourseOfferingCommandResultDto
        {
            Id = courseOffering.Id,
            CourseDefinitionId = courseOffering.CourseDefinitionId,
            AcademicYear = courseOffering.AcademicYear,
            TermName = courseOffering.TermName,
            Status = courseOffering.Status,
            IsLocked = courseOffering.IsLocked
        };
    }

    private static void ValidateInput(CourseOfferingInputBase input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (input.CourseDefinitionId == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "Course definition is required.",
                [nameof(input.CourseDefinitionId)]
            ));
        }

        var hasScheduleDayOfWeek = input.ScheduleDayOfWeek.HasValue;
        var hasScheduleStartTime = input.ScheduleStartTime.HasValue;
        var hasScheduleEndTime = input.ScheduleEndTime.HasValue;
        var hasScheduleValues = hasScheduleDayOfWeek || hasScheduleStartTime || hasScheduleEndTime;

        if (hasScheduleValues && !(hasScheduleDayOfWeek && hasScheduleStartTime && hasScheduleEndTime))
        {
            validationErrors.Add(new ValidationResult(
                "Schedule day of week, start time, and end time must be provided together or all left empty.",
                [
                    nameof(input.ScheduleDayOfWeek),
                    nameof(input.ScheduleStartTime),
                    nameof(input.ScheduleEndTime)
                ]
            ));
        }
        else if (hasScheduleValues && !Enum.IsDefined(typeof(DayOfWeek), input.ScheduleDayOfWeek!.Value))
        {
            validationErrors.Add(new ValidationResult(
                "Schedule day of week is invalid.",
                [nameof(input.ScheduleDayOfWeek)]
            ));
        }

        if (hasScheduleValues && input.ScheduleEndTime!.Value <= input.ScheduleStartTime!.Value)
        {
            validationErrors.Add(new ValidationResult(
                "Schedule end time must be later than start time.",
                [nameof(input.ScheduleEndTime)]
            ));
        }

        if (input.EnrollmentEndsAt <= input.EnrollmentStartsAt)
        {
            validationErrors.Add(new ValidationResult(
                "Enrollment ends at must be later than starts at.",
                [nameof(input.EnrollmentEndsAt)]
            ));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course offering input is invalid.", validationErrors);
        }
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            AcademicErrorCodes.CourseDefinitionNotFound => "Course definition was not found.",
            AcademicErrorCodes.CourseOfferingTermInvalid => "Term information is invalid.",
            AcademicErrorCodes.CourseOfferingScheduleSlotInvalid => "Schedule slot is invalid.",
            AcademicErrorCodes.CourseOfferingEnrollmentWindowInvalid => "Enrollment window is invalid.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Course offering operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            AcademicErrorCodes.CourseDefinitionNotFound => nameof(CourseOfferingInputBase.CourseDefinitionId),
            AcademicErrorCodes.CourseOfferingTermInvalid => nameof(CourseOfferingInputBase.TermName),
            AcademicErrorCodes.CourseOfferingScheduleSlotInvalid => nameof(CourseOfferingInputBase.ScheduleEndTime),
            AcademicErrorCodes.CourseOfferingEnrollmentWindowInvalid => nameof(CourseOfferingInputBase.EnrollmentEndsAt),
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
}
