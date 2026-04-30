using Volo.Abp;
using Volo.Abp.DependencyInjection;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.CourseOfferingManagerPartials;

namespace ZYC.VerbClass.Web.Modules.Academic.Pages.Academic.TimetableManagerPartials;

public class TimetableContentBuilder : ITransientDependency
{
    public TimetableContentBuilder(
        IAcademicTermAppService academicTermAppService,
        IAcademicTimetableAppService academicTimetableAppService)
    {
        AcademicTermAppService = academicTermAppService;
        AcademicTimetableAppService = academicTimetableAppService;
    }

    private IAcademicTermAppService AcademicTermAppService { get; }

    private IAcademicTimetableAppService AcademicTimetableAppService { get; }

    public async Task<TimetableContentModel> BuildAsync(Guid? activeTermId = null)
    {
        var terms = await BuildTermOptionsAsync();
        if (activeTermId.HasValue && terms.All(term => term.Id != activeTermId.Value))
        {
            throw new UserFriendlyException("Academic term was not found.");
        }

        return new TimetableContentModel
        {
            Terms = terms,
            ActiveTermId = activeTermId,
            Timetable = activeTermId.HasValue
                ? MapTimetable(await AcademicTimetableAppService.GetTermTimetableAsync(activeTermId.Value))
                : null
        };
    }

    private async Task<CourseOfferingTermOptionModel[]> BuildTermOptionsAsync()
    {
        return (await AcademicTermAppService.GetListAsync())
            .Select(term => new CourseOfferingTermOptionModel(
                term.Id,
                term.AcademicYear,
                term.Code,
                term.Name))
            .ToArray();
    }

    private static TimetableViewModel MapTimetable(AcademicTimetableDto timetable)
    {
        var cellsByKey = timetable.Cells.ToDictionary(x => (x.Weekday, x.PeriodNo));

        return new TimetableViewModel
        {
            AcademicTermId = timetable.AcademicTermId,
            TermTitle = FormatTermTitle(
                timetable.AcademicYear,
                timetable.AcademicTermCode,
                timetable.AcademicTermName),
            WeekdayLabels = timetable.Weekdays.Select(x => x.Label).ToArray(),
            Rows = timetable.Periods
                .Select(period => new TimetableRowModel
                {
                    Period = new TimetablePeriodModel
                    {
                        PeriodNo = period.PeriodNo,
                        Label = period.Label,
                        TimeRange = $"{period.StartTime:HH\\:mm} - {period.EndTime:HH\\:mm}"
                    },
                    Cells = timetable.Weekdays
                        .Select(weekday => MapCell(
                            timetable.AcademicTermId,
                            GetRequiredCell(cellsByKey, weekday.Weekday, period.PeriodNo)))
                        .ToArray()
                })
                .ToArray(),
            UnscheduledOfferings = timetable.UnscheduledOfferings
                .Select(offering => MapOffering(timetable.AcademicTermId, offering))
                .ToArray(),
            ScheduledOfferingCount = timetable.Cells
                .SelectMany(cell => cell.Offerings)
                .Select(offering => offering.Id)
                .Distinct()
                .Count()
        };
    }

    private static AcademicTimetableCellDto GetRequiredCell(
        IReadOnlyDictionary<(AcademicWeekday Weekday, int PeriodNo), AcademicTimetableCellDto> cellsByKey,
        AcademicWeekday weekday,
        int periodNo)
    {
        if (!cellsByKey.TryGetValue((weekday, periodNo), out var cell))
        {
            throw new UserFriendlyException("Timetable cell was not found.");
        }

        return cell;
    }

    private static TimetableCellModel MapCell(Guid academicTermId, AcademicTimetableCellDto cell)
    {
        return new TimetableCellModel
        {
            Weekday = cell.Weekday,
            PeriodNo = cell.PeriodNo,
            Offerings = cell.Offerings
                .Select(offering => MapOffering(academicTermId, offering))
                .ToArray()
        };
    }

    private static TimetableOfferingModel MapOffering(
        Guid academicTermId,
        AcademicTimetableOfferingDto offering)
    {
        if (academicTermId == Guid.Empty)
        {
            throw new UserFriendlyException("Academic term was not found.");
        }

        return new TimetableOfferingModel
        {
            Id = offering.Id,
            OfferingCode = offering.OfferingCode,
            CourseCodeSnapshot = offering.CourseCodeSnapshot,
            CourseNameSnapshot = offering.CourseNameSnapshot,
            TeacherLine = string.Join(", ", offering.TeacherNames),
            OfferingUrl = string.Empty
        };
    }

    private static string FormatTermTitle(int academicYear, string code, string name)
    {
        return $"{academicYear} {name} ({code})";
    }
}
