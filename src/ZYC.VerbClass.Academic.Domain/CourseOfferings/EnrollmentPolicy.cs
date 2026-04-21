using Volo.Abp;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class EnrollmentPolicy
{
    protected EnrollmentPolicy()
    {
    }

    public EnrollmentPolicy(DateTime opensAt, DateTime closesAt)
    {
        if (closesAt <= opensAt)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingEnrollmentWindowInvalid)
                .WithData("OpensAt", opensAt)
                .WithData("ClosesAt", closesAt);
        }

        OpensAt = opensAt;
        ClosesAt = closesAt;
    }

    public DateTime OpensAt { get; private set; }

    public DateTime ClosesAt { get; private set; }
}
