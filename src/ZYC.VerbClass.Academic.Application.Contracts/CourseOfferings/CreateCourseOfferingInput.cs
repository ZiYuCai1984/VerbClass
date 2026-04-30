using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;

public class CreateCourseOfferingInput : CourseOfferingInputBase
{
    [Required]
    public Guid? AcademicTermId { get; set; }

    [Required]
    public Guid? CourseDefinitionId { get; set; }
}
