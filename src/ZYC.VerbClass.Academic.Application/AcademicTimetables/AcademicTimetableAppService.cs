using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.AcademicTimetables;

[Authorize]
public class AcademicTimetableAppService : ApplicationService, IAcademicTimetableAppService
{
    private readonly IAcademicTermRepository _academicTermRepository;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly ICourseOfferingParticipantRepository _participantRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public AcademicTimetableAppService(
        IAcademicTermRepository academicTermRepository,
        ICourseOfferingRepository courseOfferingRepository,
        ICourseOfferingParticipantRepository participantRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _academicTermRepository = academicTermRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _participantRepository = participantRepository;
        _userRepository = userRepository;
    }

    public async Task<AcademicTimetableDto> GetTermTimetableAsync(Guid academicTermId)
    {
        if (academicTermId == Guid.Empty)
        {
            throw CreateTermNotFoundException();
        }

        var academicTerm = await _academicTermRepository.FindAsync(academicTermId)
            ?? throw CreateTermNotFoundException();

        var courseOfferings = await _courseOfferingRepository.GetListByTermAsync(academicTerm.Id);
        var teacherNamesByOfferingId = await BuildTeacherNamesByOfferingIdAsync(courseOfferings);
        var periods = academicTerm.Periods
            .OrderBy(x => x.PeriodNo)
            .ToArray();
        var weekdays = BuildWeekdays();

        return new AcademicTimetableDto
        {
            AcademicTermId = academicTerm.Id,
            AcademicYear = academicTerm.AcademicYear,
            AcademicTermCode = academicTerm.Code,
            AcademicTermName = academicTerm.Name,
            Periods = periods
                .Select(period => new AcademicTimetablePeriodDto
                {
                    PeriodNo = period.PeriodNo,
                    Label = period.Label,
                    StartTime = period.StartTime,
                    EndTime = period.EndTime
                })
                .ToArray(),
            Weekdays = weekdays,
            Cells = BuildCells(courseOfferings, teacherNamesByOfferingId, periods, weekdays),
            UnscheduledOfferings = courseOfferings
                .Where(courseOffering => courseOffering.ScheduleSlots.Count == 0)
                .OrderBy(courseOffering => courseOffering.OfferingCode, StringComparer.OrdinalIgnoreCase)
                .Select(courseOffering => MapOffering(
                    courseOffering,
                    GetTeacherNames(teacherNamesByOfferingId, courseOffering.Id)))
                .ToArray()
        };
    }

    private async Task<Dictionary<Guid, string[]>> BuildTeacherNamesByOfferingIdAsync(
        CourseOffering[] courseOfferings)
    {
        var offeringIds = courseOfferings.Select(x => x.Id).ToArray();
        var teachers = (await _participantRepository.GetListByOfferingsAsync(offeringIds))
            .Where(participant => participant.Role == CourseOfferingParticipantRole.Teacher)
            .ToArray();

        if (teachers.Length == 0)
        {
            return [];
        }

        var usersById = await BuildUsersByIdAsync(teachers.Select(x => x.UserId).Distinct().ToArray());

        return teachers
            .GroupBy(x => x.CourseOfferingId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(participant => BuildDisplayName(GetRequiredUser(usersById, participant.UserId)))
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            );
    }

    private async Task<Dictionary<Guid, IdentityUser>> BuildUsersByIdAsync(Guid[] userIds)
    {
        var users = await _userRepository.GetListAsync(user => userIds.Contains(user.Id));
        return users.ToDictionary(user => user.Id);
    }

    private static AcademicTimetableWeekdayDto[] BuildWeekdays()
    {
        return
        [
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Monday,
                Label = "Monday"
            },
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Tuesday,
                Label = "Tuesday"
            },
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Wednesday,
                Label = "Wednesday"
            },
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Thursday,
                Label = "Thursday"
            },
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Friday,
                Label = "Friday"
            },
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Saturday,
                Label = "Saturday"
            },
            new AcademicTimetableWeekdayDto
            {
                Weekday = AcademicWeekday.Sunday,
                Label = "Sunday"
            }
        ];
    }

    private static AcademicTimetableCellDto[] BuildCells(
        CourseOffering[] courseOfferings,
        IReadOnlyDictionary<Guid, string[]> teacherNamesByOfferingId,
        TermPeriodDefinition[] periods,
        AcademicTimetableWeekdayDto[] weekdays)
    {
        var periodNos = periods.Select(x => x.PeriodNo).ToHashSet();
        var offeringsBySlot = new Dictionary<(AcademicWeekday Weekday, int PeriodNo), List<CourseOffering>>();

        foreach (var courseOffering in courseOfferings)
        {
            foreach (var scheduleSlot in courseOffering.ScheduleSlots)
            {
                if (!periodNos.Contains(scheduleSlot.PeriodNo))
                {
                    throw new UserFriendlyException("Schedule period was not found in the term.");
                }

                var key = (scheduleSlot.Weekday, scheduleSlot.PeriodNo);
                if (!offeringsBySlot.TryGetValue(key, out var offerings))
                {
                    offerings = [];
                    offeringsBySlot.Add(key, offerings);
                }

                offerings.Add(courseOffering);
            }
        }

        var cells = new List<AcademicTimetableCellDto>(periods.Length * weekdays.Length);
        foreach (var period in periods)
        {
            foreach (var weekday in weekdays)
            {
                var key = (weekday.Weekday, period.PeriodNo);
                IEnumerable<CourseOffering> offerings = offeringsBySlot.TryGetValue(key, out var slotOfferings)
                    ? slotOfferings
                    : [];

                cells.Add(new AcademicTimetableCellDto
                {
                    Weekday = weekday.Weekday,
                    PeriodNo = period.PeriodNo,
                    Offerings = offerings
                        .OrderBy(courseOffering => courseOffering.OfferingCode, StringComparer.OrdinalIgnoreCase)
                        .Select(courseOffering => MapOffering(
                            courseOffering,
                            GetTeacherNames(teacherNamesByOfferingId, courseOffering.Id)))
                        .ToArray()
                });
            }
        }

        return cells.ToArray();
    }

    private static AcademicTimetableOfferingDto MapOffering(
        CourseOffering courseOffering,
        string[] teacherNames)
    {
        return new AcademicTimetableOfferingDto
        {
            Id = courseOffering.Id,
            OfferingCode = courseOffering.OfferingCode,
            CourseCodeSnapshot = courseOffering.CourseCodeSnapshot,
            CourseNameSnapshot = courseOffering.CourseNameSnapshot,
            TeacherNames = teacherNames
        };
    }

    private static string[] GetTeacherNames(
        IReadOnlyDictionary<Guid, string[]> teacherNamesByOfferingId,
        Guid courseOfferingId)
    {
        return teacherNamesByOfferingId.TryGetValue(courseOfferingId, out var teacherNames)
            ? teacherNames
            : [];
    }

    private static IdentityUser GetRequiredUser(
        IReadOnlyDictionary<Guid, IdentityUser> usersById,
        Guid userId)
    {
        if (!usersById.TryGetValue(userId, out var user))
        {
            throw new UserFriendlyException("Teacher user was not found.");
        }

        return user;
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
            ? throw new UserFriendlyException("Teacher display name could not be resolved.")
            : user.UserName;
    }

    private static UserFriendlyException CreateTermNotFoundException()
    {
        return new UserFriendlyException("Academic term was not found.");
    }
}
