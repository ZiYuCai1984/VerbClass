using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferingParticipants;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;
using ZYC.VerbClass.Academic.EntityFrameworkCore;
using ZYC.VerbClass.Domain.AppFiles;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.UserProfiles;

namespace ZYC.VerbClass.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class VerbClassDbContext :
    AbpDbContext<VerbClassDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<DepartmentAssignment> DepartmentAssignments { get; set; }
    public DbSet<AppFile> AppFiles { get; set; }
    public DbSet<AcademicTerm> AcademicTerms { get; set; }
    public DbSet<AcademicTimeTemplate> AcademicTimeTemplates { get; set; }
    public DbSet<CourseDefinition> CourseDefinitions { get; set; }
    public DbSet<CourseOffering> CourseOfferings { get; set; }
    public DbSet<CourseOfferingParticipant> CourseOfferingParticipants { get; set; }

    public VerbClassDbContext(DbContextOptions<VerbClassDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ApplyConfiguration(new DepartmentConfiguration());
        builder.ApplyConfiguration(new DepartmentAssignmentConfiguration());
        builder.ApplyConfiguration(new AppFileConfiguration());
        builder.ApplyConfiguration(new UserProfileConfiguration());
        builder.ApplyConfiguration(new AcademicTermConfiguration());
        builder.ApplyConfiguration(new AcademicTimeTemplateConfiguration());
        builder.ApplyConfiguration(new CourseDefinitionConfiguration());
        builder.ApplyConfiguration(new CourseOfferingConfiguration());
        builder.ApplyConfiguration(new CourseOfferingParticipantConfiguration());

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(VerbClassConsts.DbTablePrefix + "YourEntities", VerbClassConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
