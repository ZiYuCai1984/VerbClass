using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.Memberships;
using ZYC.VerbClass.Academic.Domain.Memberships;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.CourseMemberships;

public class CourseMembershipAppService : ApplicationService, ICourseMembershipAppService
{
    private readonly CourseMembershipManager _courseMembershipManager;
    private readonly IRepository<CourseMembership, Guid> _courseMembershipRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public CourseMembershipAppService(
        CourseMembershipManager courseMembershipManager,
        IRepository<CourseMembership, Guid> courseMembershipRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _courseMembershipManager = courseMembershipManager;
        _courseMembershipRepository = courseMembershipRepository;
        _userRepository = userRepository;
    }

    public async Task<CourseMembershipListItemDto[]> GetListAsync(Guid courseOfferingId)
    {
        if (courseOfferingId == Guid.Empty)
        {
            throw new AbpValidationException(
                "Course offering is required.",
                [new ValidationResult("Course offering is required.", [nameof(courseOfferingId)])]
            );
        }

        var memberships = await _courseMembershipRepository.GetListAsync(
            x => x.CourseOfferingId == courseOfferingId
        );

        var usersById = await GetUsersByIdAsync(memberships.Select(x => x.UserId));

        return memberships
            .OrderBy(x => x.Role)
            .ThenBy(x => x.Status)
            .ThenBy(x => usersById.TryGetValue(x.UserId, out var user) ? BuildDisplayName(user) : x.UserId.ToString())
            .ThenBy(x => x.Id)
            .Select(x => MapListItem(x, usersById.GetValueOrDefault(x.UserId)))
            .ToArray();
    }

    public async Task<CourseMembershipCommandResultDto> JoinStudentAsync(JoinCourseMembershipInput input)
    {
        ValidateJoinInput(input);

        var user = await _userRepository.FindAsync(input.UserId)
            ?? throw CreateValidationException(new BusinessException(AcademicErrorCodes.MembershipUserNotFound));

        var existingMembership = await _courseMembershipRepository.FindAsync(
            x => x.CourseOfferingId == input.CourseOfferingId && x.UserId == input.UserId
        );

        try
        {
            var membership = await _courseMembershipManager.JoinStudentAsync(
                input.CourseOfferingId,
                user.Id
            );

            if (existingMembership is null)
            {
                await _courseMembershipRepository.InsertAsync(membership, true);
            }
            else
            {
                await _courseMembershipRepository.UpdateAsync(membership, true);
            }

            return MapCommandResult(membership);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseMembershipCommandResultDto> DropAsync(Guid id)
    {
        var membership = await _courseMembershipRepository.FindAsync(id)
            ?? throw CreateMembershipNotFoundException();

        try
        {
            membership.Drop();
            await _courseMembershipRepository.UpdateAsync(membership, true);

            return MapCommandResult(membership);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseMembershipCommandResultDto> UpdateRoleAsync(Guid id, UpdateCourseMembershipRoleInput input)
    {
        ValidateRoleInput(input);

        var membership = await _courseMembershipRepository.FindAsync(id)
            ?? throw CreateMembershipNotFoundException();

        try
        {
            membership.ChangeRole(input.Role);
            await _courseMembershipRepository.UpdateAsync(membership, true);

            return MapCommandResult(membership);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    private async Task<IReadOnlyDictionary<Guid, IdentityUser>> GetUsersByIdAsync(IEnumerable<Guid> userIds)
    {
        var normalizedUserIds = userIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedUserIds.Length == 0)
        {
            return new Dictionary<Guid, IdentityUser>();
        }

        var users = await _userRepository.GetListAsync(x => normalizedUserIds.Contains(x.Id));
        return users.ToDictionary(x => x.Id);
    }

    private static CourseMembershipListItemDto MapListItem(
        CourseMembership membership,
        IdentityUser? user)
    {
        return new CourseMembershipListItemDto
        {
            Id = membership.Id,
            CourseOfferingId = membership.CourseOfferingId,
            UserId = membership.UserId,
            UserName = user?.UserName ?? string.Empty,
            DisplayName = BuildDisplayName(user),
            Email = user?.Email,
            Role = membership.Role,
            Status = membership.Status
        };
    }

    private static CourseMembershipCommandResultDto MapCommandResult(CourseMembership membership)
    {
        return new CourseMembershipCommandResultDto
        {
            Id = membership.Id,
            CourseOfferingId = membership.CourseOfferingId,
            UserId = membership.UserId,
            Role = membership.Role,
            Status = membership.Status
        };
    }

    private static string BuildDisplayName(IdentityUser? user)
    {
        if (user is null)
        {
            return "Unknown";
        }

        var parts = new[]
        {
            user.Surname?.Trim(),
            user.Name?.Trim()
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        var displayName = string.Join(' ', parts);
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return string.IsNullOrWhiteSpace(user.UserName)
            ? "Unknown"
            : user.UserName;
    }

    private static void ValidateJoinInput(JoinCourseMembershipInput input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (input.CourseOfferingId == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "Course offering is required.",
                [nameof(input.CourseOfferingId)]
            ));
        }

        if (input.UserId == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "User is required.",
                [nameof(input.UserId)]
            ));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course membership input is invalid.", validationErrors);
        }
    }

    private static void ValidateRoleInput(UpdateCourseMembershipRoleInput input)
    {
        if (Enum.IsDefined(typeof(CourseMembershipRole), input.Role))
        {
            return;
        }

        throw new AbpValidationException(
            "Membership role is invalid.",
            [new ValidationResult("Membership role is invalid.", [nameof(input.Role)])]
        );
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            AcademicErrorCodes.CourseOfferingNotFound => "Course offering was not found.",
            AcademicErrorCodes.CourseOfferingLocked => "Course offering is locked.",
            AcademicErrorCodes.MembershipAlreadyExists => "Membership already exists for the selected user.",
            AcademicErrorCodes.MembershipUserNotFound => "User was not found.",
            AcademicErrorCodes.MembershipEnrollmentNotOpen => "Enrollment is not currently open for this course offering.",
            AcademicErrorCodes.InvalidMembershipRole => "Membership role is invalid.",
            AcademicErrorCodes.InvalidMembershipStatus => "Membership status does not allow this operation.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Course membership operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            AcademicErrorCodes.CourseOfferingNotFound or
                AcademicErrorCodes.CourseOfferingLocked or
                AcademicErrorCodes.MembershipEnrollmentNotOpen => nameof(JoinCourseMembershipInput.CourseOfferingId),
            AcademicErrorCodes.MembershipAlreadyExists or
                AcademicErrorCodes.MembershipUserNotFound => nameof(JoinCourseMembershipInput.UserId),
            AcademicErrorCodes.InvalidMembershipRole => nameof(UpdateCourseMembershipRoleInput.Role),
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

    private static UserFriendlyException CreateMembershipNotFoundException()
    {
        return new UserFriendlyException("Course membership was not found.");
    }
}
