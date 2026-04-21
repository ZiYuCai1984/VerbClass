namespace ZYC.VerbClass.Application.Contracts.Users;

public class UserCommandResultDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}
