using Microsoft.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

public static class AcademicModelBuilderExtensions
{
    public static void ConfigureAcademic(this ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = AcademicDbProperties.DbTablePrefix;
        _ = AcademicDbProperties.DbSchema;

        builder.ApplyConfiguration(new CourseDefinitionConfiguration());
        builder.ApplyConfiguration(new CourseOfferingConfiguration());
        builder.ApplyConfiguration(new CourseMembershipConfiguration());
    }
}
