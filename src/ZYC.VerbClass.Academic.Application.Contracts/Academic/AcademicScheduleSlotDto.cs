using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.Academic;

public class AcademicScheduleSlotDto
{
    [EnumDataType(typeof(AcademicWeekday))]
    public AcademicWeekday Weekday { get; set; }

    [Range(1, int.MaxValue)]
    public int PeriodNo { get; set; }
}
