using Volo.Abp;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.AcademicTerms;

public class TermPeriodDefinition
{
    protected TermPeriodDefinition()
    {
    }

    private TermPeriodDefinition(
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

    public static TermPeriodDefinition Create(
        int periodNo,
        string label,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return new TermPeriodDefinition(periodNo, label, startTime, endTime);
    }

    private static int NormalizePeriodNo(int periodNo)
    {
        if (periodNo <= 0)
        {
            throw new BusinessException(AcademicTermErrorCodes.InvalidPeriodNo)
                .WithData(nameof(PeriodNo), periodNo);
        }

        return periodNo;
    }

    private static string NormalizeLabel(string label)
    {
        return Check.NotNullOrWhiteSpace(
                label,
                nameof(label),
                AcademicTermConsts.MaxPeriodLabelLength
            )
            .Trim();
    }

    private void SetTimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
        {
            throw new BusinessException(AcademicTermErrorCodes.InvalidPeriodTimeRange)
                .WithData(nameof(StartTime), startTime)
                .WithData(nameof(EndTime), endTime);
        }

        StartTime = startTime;
        EndTime = endTime;
    }
}
