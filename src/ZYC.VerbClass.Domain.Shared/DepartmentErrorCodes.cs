namespace ZYC.VerbClass.Domain.Shared;

public static class DepartmentErrorCodes
{
    public const string CodeAlreadyExists = "VerbClass:Department:CodeAlreadyExists";
    public const string ParentCannotBeSelf = "VerbClass:Department:ParentCannotBeSelf";
    public const string InvalidEffectivePeriod = "VerbClass:Department:InvalidEffectivePeriod";
    public const string ParentNotFound = "VerbClass:Department:ParentNotFound";
    public const string ParentIsInactive = "VerbClass:Department:ParentIsInactive";
    public const string CircularHierarchy = "VerbClass:Department:CircularHierarchy";
}