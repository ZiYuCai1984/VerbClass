using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;

public class UpdateAcademicTermInput
{
    [Range(AcademicTermConsts.MinAcademicYear, AcademicTermConsts.MaxAcademicYear)]
    public int AcademicYear { get; set; }

    [Required]
    [StringLength(AcademicTermConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(AcademicTermConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; }
}
