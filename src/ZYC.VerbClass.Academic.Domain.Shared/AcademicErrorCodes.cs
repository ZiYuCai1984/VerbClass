namespace ZYC.VerbClass.Academic.Domain.Shared;

public static class AcademicErrorCodes
{
    public const string CourseDefinitionNotFound = "Academic:CourseDefinition:NotFound";
    public const string CourseDefinitionCodeAlreadyExists = "Academic:CourseDefinition:CodeAlreadyExists";

    public const string CourseOfferingNotFound = "Academic:CourseOffering:NotFound";
    public const string CourseOfferingLocked = "Academic:CourseOffering:Locked";
    public const string CourseOfferingTermInvalid = "Academic:CourseOffering:TermInvalid";
    public const string CourseOfferingScheduleSlotInvalid = "Academic:CourseOffering:ScheduleSlotInvalid";
    public const string CourseOfferingEnrollmentWindowInvalid = "Academic:CourseOffering:EnrollmentWindowInvalid";
    public const string InvalidCourseOfferingStatus = "Academic:CourseOffering:InvalidStatus";

    public const string MembershipNotFound = "Academic:Membership:NotFound";
    public const string MembershipAlreadyExists = "Academic:Membership:AlreadyExists";
    public const string MembershipUserNotFound = "Academic:Membership:UserNotFound";
    public const string MembershipEnrollmentNotOpen = "Academic:Membership:EnrollmentNotOpen";
    public const string InvalidMembershipRole = "Academic:Membership:InvalidRole";
    public const string InvalidMembershipStatus = "Academic:Membership:InvalidStatus";

    public const string AttendanceSessionNotFound = "Academic:AttendanceSession:NotFound";
    public const string InvalidAttendanceSessionStatus = "Academic:AttendanceSession:InvalidStatus";
    public const string AttendanceRecordAlreadyExists = "Academic:AttendanceRecord:AlreadyExists";
    public const string InvalidAttendanceState = "Academic:AttendanceRecord:InvalidState";
}
