using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;

public class ChangeCourseOfferingParticipantRoleInput
{
    [EnumDataType(typeof(CourseOfferingParticipantRole))]
    public CourseOfferingParticipantRole Role { get; set; }
}
