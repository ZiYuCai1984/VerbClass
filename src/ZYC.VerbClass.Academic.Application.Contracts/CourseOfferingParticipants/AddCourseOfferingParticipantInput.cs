using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;

public class AddCourseOfferingParticipantInput
{
    [Required]
    public Guid? CourseOfferingId { get; set; }

    [Required]
    public Guid? UserId { get; set; }

    [EnumDataType(typeof(CourseOfferingParticipantRole))]
    public CourseOfferingParticipantRole Role { get; set; }
}
