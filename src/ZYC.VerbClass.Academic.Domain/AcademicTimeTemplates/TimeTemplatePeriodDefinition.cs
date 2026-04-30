using Volo.Abp;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;

public class TimeTemplatePeriodDefinition
{
    protected TimeTemplatePeriodDefinition()
    {
    }

    private TimeTemplatePeriodDefinition(
        int periodNo,
        string label,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        PeriodNo = NormalizePeriodNo(periodNo);
        Label = NormalizeLabel(label);
        SetTimeRange(startTime, endTime);
    }

    public int PeriodNo { get; private set; }

    public string Label { get; private set; } = string.Empty;

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public static TimeTemplatePeriodDefinition Create(
        int periodNo,
        string label,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return new TimeTemplatePeriodDefinition(periodNo, label, startTime, endTime);
    }

    private static int NormalizePeriodNo(int periodNo)
    {
        if (periodNo <= 0)
        {
            throw new BusinessException(AcademicTimeTemplateErrorCodes.InvalidPeriodNo)
                .WithData(nameof(PeriodNo), periodNo);
        }

        return periodNo;
    }

    private static string NormalizeLabel(string label)
    {
        return Check.NotNullOrWhiteSpace(
                label,
                nameof(label),
                AcademicTimeTemplateConsts.MaxPeriodLabelLength
            )
            .Trim();
    }

    private void SetTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
        {
            throw new BusinessException(AcademicTimeTemplateErrorCodes.InvalidPeriodTimeRange)
                .WithData(nameof(StartTime), startTime)
                .WithData(nameof(EndTime), endTime);
        }

        StartTime = startTime;
        EndTime = endTime;
    }
}
