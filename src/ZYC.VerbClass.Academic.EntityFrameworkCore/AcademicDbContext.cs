using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;

namespace ZYC.VerbClass.Academic.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class AcademicDbContext : AbpDbContext<AcademicDbContext>
{
    public DbSet<CourseDefinition> CourseDefinitions { get; set; }

    public DbSet<CourseOfferingParticipant> CourseOfferingParticipants { get; set; }

    public DbSet<CourseOffering> CourseOfferings { get; set; }

    public DbSet<AcademicTimeTemplate> AcademicTimeTemplates { get; set; }

    public DbSet<AcademicTerm> AcademicTerms { get; set; }

    public AcademicDbContext(DbContextOptions<AcademicDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new CourseDefinitionConfiguration());
        builder.ApplyConfiguration(new CourseOfferingParticipantConfiguration());
        builder.ApplyConfiguration(new CourseOfferingConfiguration());
        builder.ApplyConfiguration(new AcademicTimeTemplateConfiguration());
        builder.ApplyConfiguration(new AcademicTermConfiguration());
    }
}
