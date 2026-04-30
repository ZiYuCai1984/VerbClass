using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.CourseOfferingParticipants;

[Authorize]
public class CourseOfferingParticipantAppService
    : ApplicationService,
        ICourseOfferingParticipantAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly ICourseOfferingParticipantRepository _participantRepository;
    private readonly CourseOfferingParticipantManager _participantManager;

    public CourseOfferingParticipantAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IdentityUserManager userManager,
        ICourseOfferingRepository courseOfferingRepository,
        ICourseOfferingParticipantRepository participantRepository,
        CourseOfferingParticipantManager participantManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _courseOfferingRepository = courseOfferingRepository;
        _participantRepository = participantRepository;
        _participantManager = participantManager;
    }

    public async Task<CourseOfferingParticipantListItemDto[]> GetListAsync(Guid courseOfferingId)
    {
        await EnsureCourseOfferingExistsAsync(courseOfferingId);

        var participants = await _participantRepository.GetListByOfferingAsync(courseOfferingId);
        if (participants.Length == 0)
        {
            return [];
        }

        var usersById = await BuildUsersByIdAsync(participants.Select(x => x.UserId).ToArray());

        return participants
            .OrderBy(x => x.Role)
            .ThenBy(x => BuildDisplayName(GetRequiredUser(usersById, x.UserId)), StringComparer.OrdinalIgnoreCase)
            .Select(participant =>
            {
                var user = GetRequiredUser(usersById, participant.UserId);

                return new CourseOfferingParticipantListItemDto
                {
                    Id = participant.Id,
                    CourseOfferingId = participant.CourseOfferingId,
                    UserId = participant.UserId,
                    DisplayName = BuildDisplayName(user),
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email,
                    Role = participant.Role
                };
            })
            .ToArray();
    }

    public async Task<CourseOfferingParticipantUserOptionDto[]> GetAssignableUsersAsync(Guid courseOfferingId)
    {
        await EnsureCourseOfferingExistsAsync(courseOfferingId);

        var assignedUserIds = (await _participantRepository.GetListByOfferingAsync(courseOfferingId))
            .Select(x => x.UserId)
            .ToHashSet();

        var users = (await _userRepository.GetListAsync())
            .Where(user => user.IsActive && !assignedUserIds.Contains(user.Id))
            .OrderBy(BuildDisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.UserName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var options = new List<CourseOfferingParticipantUserOptionDto>(users.Length);
        foreach (var user in users)
        {
            options.Add(new CourseOfferingParticipantUserOptionDto
            {
                UserId = user.Id,
                DisplayName = BuildDisplayName(user),
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                RoleNames = (await _userManager.GetRolesAsync(user))
                    .Where(VerbClassRoles.IsManagedRole)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            });
        }

        return options.ToArray();
    }

    public async Task<CourseOfferingParticipantCommandResultDto> AddAsync(
        AddCourseOfferingParticipantInput input)
    {
        ValidateAddInput(input);

        var courseOffering = await _courseOfferingRepository.FindAsync(input.CourseOfferingId!.Value)
            ?? throw CreateCourseOfferingNotFoundException();
        var user = await GetActiveUserAsync(input.UserId!.Value);

        try
        {
            await EnsureUserQualifiedForRoleAsync(user, input.Role);

            var participant = await _participantManager.CreateAsync(
                courseOffering,
                user.Id,
                input.Role
            );

            await _participantRepository.InsertAsync(participant, true);

            return MapCommandResult(participant, user);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseOfferingParticipantCommandResultDto> ChangeRoleAsync(
        Guid participantId,
        ChangeCourseOfferingParticipantRoleInput input)
    {
        ValidateChangeRoleInput(input);

        var participant = await _participantRepository.FindAsync(participantId)
            ?? throw CreateParticipantNotFoundException();
        var user = await GetActiveUserAsync(participant.UserId);

        try
        {
            await EnsureUserQualifiedForRoleAsync(user, input.Role);
            await _participantManager.ChangeRoleAsync(participant, input.Role);

            await _participantRepository.UpdateAsync(participant, true);

            return MapCommandResult(participant, user);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    public async Task<CourseOfferingParticipantCommandResultDto> RemoveAsync(Guid participantId)
    {
        var participant = await _participantRepository.FindAsync(participantId)
            ?? throw CreateParticipantNotFoundException();
        var user = await GetRequiredUserAsync(participant.UserId);
        var result = MapCommandResult(participant, user);

        await _participantRepository.DeleteAsync(participant, true);

        return result;
    }

    private async Task EnsureCourseOfferingExistsAsync(Guid courseOfferingId)
    {
        if (courseOfferingId == Guid.Empty)
        {
            throw CreateCourseOfferingNotFoundException();
        }

        _ = await _courseOfferingRepository.FindAsync(courseOfferingId)
            ?? throw CreateCourseOfferingNotFoundException();
    }

    private async Task<IdentityUser> GetActiveUserAsync(Guid userId)
    {
        var user = await GetRequiredUserAsync(userId);
        if (!user.IsActive)
        {
            throw new AbpValidationException(
                "User is inactive.",
                [new ValidationResult("User is inactive.", [nameof(AddCourseOfferingParticipantInput.UserId)])]
            );
        }

        return user;
    }

    private async Task<IdentityUser> GetRequiredUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw CreateUserNotFoundException();
        }

        return await _userRepository.FindAsync(userId)
               ?? throw CreateUserNotFoundException();
    }

    private async Task<Dictionary<Guid, IdentityUser>> BuildUsersByIdAsync(Guid[] userIds)
    {
        var users = await _userRepository.GetListAsync(user => userIds.Contains(user.Id));
        return users.ToDictionary(user => user.Id);
    }

    private static IdentityUser GetRequiredUser(
        IReadOnlyDictionary<Guid, IdentityUser> usersById,
        Guid userId)
    {
        if (!usersById.TryGetValue(userId, out var user))
        {
            throw new UserFriendlyException("Participant user was not found.");
        }

        return user;
    }

    private async Task EnsureUserQualifiedForRoleAsync(
        IdentityUser user,
        CourseOfferingParticipantRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new BusinessException(CourseOfferingParticipantErrorCodes.InvalidRole)
                .WithData(nameof(CourseOfferingParticipant.Role), role);
        }

        var requiredSystemRole = GetRequiredSystemRoleName(role);
        if (requiredSystemRole is null)
        {
            return;
        }

        if (!await _userManager.IsInRoleAsync(user, requiredSystemRole))
        {
            throw new BusinessException(CourseOfferingParticipantErrorCodes.UserRoleNotQualified)
                .WithData(nameof(CourseOfferingParticipant.Role), role)
                .WithData("RequiredSystemRole", requiredSystemRole)
                .WithData(nameof(CourseOfferingParticipant.UserId), user.Id);
        }
    }

    private static string? GetRequiredSystemRoleName(CourseOfferingParticipantRole role)
    {
        return role switch
        {
            CourseOfferingParticipantRole.Teacher => VerbClassRoles.Instructor,
            CourseOfferingParticipantRole.Assistant => VerbClassRoles.Assistant,
            CourseOfferingParticipantRole.Student => VerbClassRoles.Student,
            CourseOfferingParticipantRole.Observer => null,
            _ => throw new BusinessException(CourseOfferingParticipantErrorCodes.InvalidRole)
                .WithData(nameof(CourseOfferingParticipant.Role), role)
        };
    }

    private static CourseOfferingParticipantCommandResultDto MapCommandResult(
        CourseOfferingParticipant participant,
        IdentityUser user)
    {
        return new CourseOfferingParticipantCommandResultDto
        {
            Id = participant.Id,
            CourseOfferingId = participant.CourseOfferingId,
            UserId = participant.UserId,
            DisplayName = BuildDisplayName(user),
            Role = participant.Role
        };
    }

    private static string BuildDisplayName(IdentityUser user)
    {
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
            ? throw new UserFriendlyException("User display name could not be resolved.")
            : user.UserName;
    }

    private static void ValidateAddInput(AddCourseOfferingParticipantInput input)
    {
        var validationErrors = ValidateInput(input);

        if (!input.CourseOfferingId.HasValue || input.CourseOfferingId.Value == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "Course offering is required.",
                [nameof(input.CourseOfferingId)]
            ));
        }

        if (!input.UserId.HasValue || input.UserId.Value == Guid.Empty)
        {
            validationErrors.Add(new ValidationResult(
                "User is required.",
                [nameof(input.UserId)]
            ));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course participant input is invalid.", validationErrors);
        }
    }

    private static void ValidateChangeRoleInput(ChangeCourseOfferingParticipantRoleInput input)
    {
        var validationErrors = ValidateInput(input);
        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Course participant input is invalid.", validationErrors);
        }
    }

    private static List<ValidationResult> ValidateInput(object input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);
        return validationErrors;
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            CourseOfferingParticipantErrorCodes.InvalidRole => "Course participant role is invalid.",
            CourseOfferingParticipantErrorCodes.UserAlreadyAssigned => "This user is already assigned to the offering.",
            CourseOfferingParticipantErrorCodes.UserRoleNotQualified => "User does not have the required system role.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Course participant operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            CourseOfferingParticipantErrorCodes.InvalidRole or
                CourseOfferingParticipantErrorCodes.UserRoleNotQualified => nameof(AddCourseOfferingParticipantInput.Role),
            CourseOfferingParticipantErrorCodes.UserAlreadyAssigned => nameof(AddCourseOfferingParticipantInput.UserId),
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

    private static UserFriendlyException CreateParticipantNotFoundException()
    {
        return new UserFriendlyException("Course participant was not found.");
    }

    private static UserFriendlyException CreateUserNotFoundException()
    {
        return new UserFriendlyException("User was not found.");
    }
}
