using System.ComponentModel.DataAnnotations;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;

public abstract class CourseDefinitionInputBase
{
    [Required]
    [StringLength(CourseDefinitionConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(CourseDefinitionConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(CourseDefinitionConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
