using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using ZYC.VerbClass.Academic.Application.Contracts.MyCourses;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Memberships;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.MyCourses;

[Authorize]
public class MyCourseAppService : ApplicationService, IMyCourseAppService
{
    private readonly IRepository<CourseMembership, Guid> _courseMembershipRepository;
    private readonly IRepository<CourseOffering, Guid> _courseOfferingRepository;
    private readonly IRepository<CourseDefinition, Guid> _courseDefinitionRepository;

    public MyCourseAppService(
        IRepository<CourseMembership, Guid> courseMembershipRepository,
        IRepository<CourseOffering, Guid> courseOfferingRepository,
        IRepository<CourseDefinition, Guid> courseDefinitionRepository)
    {
        _courseMembershipRepository = courseMembershipRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _courseDefinitionRepository = courseDefinitionRepository;
    }

    public async Task<MyCourseListItemDto[]> GetListAsync()
    {
        var currentUserId = CurrentUser.Id
            ?? throw new AbpAuthorizationException("Current user is not authenticated.");

        var memberships = await _courseMembershipRepository.GetListAsync(
            x => x.UserId == currentUserId && x.Status != CourseMembershipStatus.Removed
        );

        if (memberships.Count == 0)
        {
            return [];
        }

        var offeringsById = await GetOfferingsByIdAsync(memberships.Select(x => x.CourseOfferingId));
        var definitionsById = await GetDefinitionsByIdAsync(
            offeringsById.Values.Select(x => x.CourseDefinitionId)
        );

        return memberships
            .Select(membership =>
            {
                offeringsById.TryGetValue(membership.CourseOfferingId, out var offering);
                var definition = offering is null
                    ? null
                    : definitionsById.GetValueOrDefault(offering.CourseDefinitionId);

                return MapListItem(membership, offering, definition);
            })
            .OrderBy(x => GetMembershipStatusOrder(x.MembershipStatus))
            .ThenByDescending(x => x.AcademicYear)
            .ThenBy(x => x.TermName)
            .ThenBy(x => x.CourseCode)
            .ThenBy(x => x.CourseName)
            .ThenBy(x => x.CourseOfferingId)
            .ToArray();
    }

    private async Task<IReadOnlyDictionary<Guid, CourseOffering>> GetOfferingsByIdAsync(
        IEnumerable<Guid> courseOfferingIds)
    {
        var normalizedIds = courseOfferingIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedIds.Length == 0)
        {
            return new Dictionary<Guid, CourseOffering>();
        }

        var offerings = await _courseOfferingRepository.GetListAsync(
            x => normalizedIds.Contains(x.Id)
        );

        return offerings.ToDictionary(x => x.Id);
    }

    private async Task<IReadOnlyDictionary<Guid, CourseDefinition>> GetDefinitionsByIdAsync(
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

        var definitions = await _courseDefinitionRepository.GetListAsync(
            x => normalizedIds.Contains(x.Id)
        );

        return definitions.ToDictionary(x => x.Id);
    }

    private static MyCourseListItemDto MapListItem(
        CourseMembership membership,
        CourseOffering? offering,
        CourseDefinition? definition)
    {
        return new MyCourseListItemDto
        {
            CourseOfferingId = membership.CourseOfferingId,
            CourseDefinitionId = offering?.CourseDefinitionId ?? Guid.Empty,
            CourseCode = definition?.Code ?? string.Empty,
            CourseName = definition?.Name ?? string.Empty,
            CourseShortName = definition?.ShortName,
            AcademicYear = offering?.AcademicYear ?? 0,
            TermName = offering?.TermName ?? string.Empty,
            ScheduleDayOfWeek = offering?.ScheduleDayOfWeek,
            ScheduleStartTime = offering?.ScheduleStartTime,
            ScheduleEndTime = offering?.ScheduleEndTime,
            ScheduleLocation = offering?.ScheduleLocation,
            OfferingStatus = offering?.Status ?? CourseOfferingStatus.Draft,
            IsLocked = offering?.IsLocked ?? false,
            MembershipRole = membership.Role,
            MembershipStatus = membership.Status
        };
    }

    private static int GetMembershipStatusOrder(CourseMembershipStatus status)
    {
        return status switch
        {
            CourseMembershipStatus.Active => 0,
            CourseMembershipStatus.Pending => 1,
            CourseMembershipStatus.Dropped => 2,
            CourseMembershipStatus.Removed => 3,
            _ => 9
        };
    }
}
