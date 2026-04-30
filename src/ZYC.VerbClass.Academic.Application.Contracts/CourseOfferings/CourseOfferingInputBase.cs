using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public abstract class CourseOfferingInputBase
{
    [Required]
    [StringLength(CourseOfferingConsts.MaxOfferingCodeLength)]
    public string OfferingCode { get; set; } = string.Empty;

    [Required]
    public AcademicScheduleSlotDto[] ScheduleSlots { get; set; } = [];
}
