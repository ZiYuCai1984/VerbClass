using Volo.Abp;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class ScheduleSlot
{
    protected ScheduleSlot()
    {
    }

    public ScheduleSlot(
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        string? location = null)
    {
        if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek) || endTime <= startTime)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingScheduleSlotInvalid)
                .WithData("DayOfWeek", (int)dayOfWeek)
                .WithData("StartTime", startTime)
                .WithData("EndTime", endTime);
        }

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        SetLocation(location);
    }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeSpan StartTime { get; private set; }

    public TimeSpan EndTime { get; private set; }

    public string? Location { get; private set; }

    private void SetLocation(string? location)
    {
        if (location.IsNullOrWhiteSpace())
        {
            Location = null;
            return;
        }

        Location = Check.Length(
            location.Trim(),
            nameof(location),
            CourseOfferingConsts.MaxLocationLength
        );
    }
}
