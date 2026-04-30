using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;

public class CreateAcademicTermInput
{
    [Range(AcademicTermConsts.MinAcademicYear, AcademicTermConsts.MaxAcademicYear)]
    public int AcademicYear { get; set; }

    [Required]
    [StringLength(AcademicTermConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(AcademicTermConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid? TimeTemplateId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; }
}
