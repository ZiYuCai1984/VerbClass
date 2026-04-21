using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Application.Contracts.Users;

public class CreateUserInput : UserInputBase
{
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
