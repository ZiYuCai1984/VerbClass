using System.ComponentModel.DataAnnotations;

namespace ZYC.VerbClass.Application.Contracts.Users;

public class UpdateUserInput : UserInputBase
{
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
}
