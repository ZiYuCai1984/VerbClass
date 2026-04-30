namespace ZYC.VerbClass.Academic.Domain.Shared;

public static class CourseOfferingErrorCodes
{
    public const string OfferingCodeAlreadyExists = "Academic:CourseOffering:OfferingCodeAlreadyExists";

    public const string InvalidWeekday = "Academic:CourseOffering:InvalidWeekday";

    public const string InvalidPeriodNo = "Academic:CourseOffering:InvalidPeriodNo";

    public const string UnknownTermPeriodNo = "Academic:CourseOffering:UnknownTermPeriodNo";

    public const string DuplicateScheduleSlot = "Academic:CourseOffering:DuplicateScheduleSlot";
}
