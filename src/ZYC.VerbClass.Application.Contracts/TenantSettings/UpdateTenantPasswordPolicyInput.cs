using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Application.Contracts.TenantSettings;

public class UpdateTenantPasswordPolicyInput
{
    [Display(Name = "Require digit")]
    public bool RequireDigit { get; set; }

    [Display(Name = "Require lowercase")]
    public bool RequireLowercase { get; set; }

    [Display(Name = "Require uppercase")]
    public bool RequireUppercase { get; set; }

    [Display(Name = "Require non-alphanumeric")]
    public bool RequireNonAlphanumeric { get; set; }

    [Display(Name = "Required length")]
    [Range(1, 128)]
    public int RequiredLength { get; set; }

    [Display(Name = "Required unique characters")]
    [Range(1, 128)]
    public int RequiredUniqueChars { get; set; }
}
