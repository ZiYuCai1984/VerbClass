using Volo.Abp;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.Domain.CourseOfferings;

public class AcademicTerm
{
    protected AcademicTerm()
    {
    }

    public AcademicTerm(int academicYear, string termName)
    {
        if (academicYear < CourseOfferingConsts.MinAcademicYear ||
            academicYear > CourseOfferingConsts.MaxAcademicYear)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingTermInvalid)
                .WithData("AcademicYear", academicYear);
        }

        if (termName.IsNullOrWhiteSpace())
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingTermInvalid)
                .WithData("TermName", termName ?? string.Empty);
        }

        var normalizedTermName = termName.Trim();
        if (normalizedTermName.Length > CourseOfferingConsts.MaxTermNameLength)
        {
            throw new BusinessException(AcademicErrorCodes.CourseOfferingTermInvalid)
                .WithData("TermName", normalizedTermName);
        }

        AcademicYear = academicYear;
        TermName = normalizedTermName;
    }

    public int AcademicYear { get; private set; }

    public string TermName { get; private set; } = string.Empty;
}
