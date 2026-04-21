using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Web.Pages.Account;

public partial class LoginModel
{
    public class InputModel
    {
        [Display(Name = "Tenant")] public string? TenantName { get; set; }

        [Required]
        [Display(Name = "Username or email")]
        public string UserNameOrEmail { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me")] public bool RememberMe { get; set; } = true;
    }
}