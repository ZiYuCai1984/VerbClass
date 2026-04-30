using Volo.Abp;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class CourseOfferingScheduleSlot
{
    protected CourseOfferingScheduleSlot()
    {
    }

    private CourseOfferingScheduleSlot(AcademicWeekday weekday, int periodNo)
    {
        Weekday = NormalizeWeekday(weekday);
        PeriodNo = NormalizePeriodNo(periodNo);
    }

    public AcademicWeekday Weekday { get; private set; }

    public int PeriodNo { get; private set; }

    public static CourseOfferingScheduleSlot Create(AcademicWeekday weekday, int periodNo)
    {
        return new CourseOfferingScheduleSlot(weekday, periodNo);
    }

    internal static AcademicWeekday NormalizeWeekday(AcademicWeekday weekday)
    {
        if (!Enum.IsDefined(weekday))
        {
            throw new BusinessException(CourseOfferingErrorCodes.InvalidWeekday)
                .WithData(nameof(Weekday), weekday);
        }

        return weekday;
    }

    internal static int NormalizePeriodNo(int periodNo)
    {
        if (periodNo <= 0)
        {
            throw new BusinessException(CourseOfferingErrorCodes.InvalidPeriodNo)
                .WithData(nameof(PeriodNo), periodNo);
        }

        return periodNo;
    }
}
