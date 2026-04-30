using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;

public class UpdateCurrentAcademicTermInput
{
    [Display(Name = "Current academic term")]
    public Guid? CurrentTermId { get; set; }
}
