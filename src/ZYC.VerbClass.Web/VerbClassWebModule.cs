using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using OpenIddict.Server.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.Users;
using ZYC.VerbClass.Academic.Application;
using ZYC.VerbClass.Academic.Application.Contracts;
using ZYC.VerbClass.Academic.Domain;
using ZYC.VerbClass.Academic.Domain.Shared;
using ZYC.VerbClass.Academic.EntityFrameworkCore;
using ZYC.VerbClass.Academic.HttpApi;
using ZYC.VerbClass.Application;
using ZYC.VerbClass.Application.Contracts;
using ZYC.VerbClass.Domain;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Domain.Shared.Localization;
using ZYC.VerbClass.EntityFrameworkCore;
using ZYC.VerbClass.HttpApi;
using ZYC.VerbClass.Web.Modules.Academic;
using ZYC.VerbClass.Web.Modules.Mock;

namespace ZYC.VerbClass.Web;

[DependsOn(
    typeof(AcademicModule),
    typeof(MockModule),
    typeof(AcademicHttpApiModule),
    typeof(AcademicApplicationModule),
    typeof(AcademicEntityFrameworkCoreModule),
    typeof(VerbClassHttpApiModule),
    typeof(VerbClassApplicationModule),
    typeof(VerbClassEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpFeatureManagementWebModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
)]
public partial class VerbClassWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(VerbClassResource),
                typeof(AcademicDomainModule).Assembly,
                typeof(AcademicDomainSharedModule).Assembly,
                typeof(AcademicApplicationModule).Assembly,
                typeof(AcademicApplicationContractsModule).Assembly,
                typeof(VerbClassDomainModule).Assembly,
                typeof(VerbClassDomainSharedModule).Assembly,
                typeof(VerbClassApplicationModule).Assembly,
                typeof(VerbClassApplicationContractsModule).Assembly,
                typeof(VerbClassWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("VerbClass");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        //if (!hostingEnvironment.IsDevelopment())
        //{
        //    PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
        //    {
        //        options.AddDevelopmentEncryptionAndSigningCertificate = false;
        //    });

        //    PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
        //    {
        //        serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx",
        //            configuration["AuthServer:CertificatePassPhrase"]!);
        //        serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
        //    });
        //}
    }


    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.AddHttpContextAccessor();

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            IdentityModelEventSource.ShowPII = true;
            IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });

            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        context.Services.AddRazorPages()
            .AddRazorRuntimeCompilation()
            .AddRazorPagesOptions(options =>
            {
                //TODO-zyc IgnoreAntiforgeryTokenAttribute
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());

                ConfigurePageRoutes(options);
            });

        ConfigureConnectionString();
        ConfigureUrls(configuration);
        ConfigureAuthentication(context);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);


        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });


        Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = true;
        });


        var containerBuilder = context.Services.GetContainerBuilder();
        containerBuilder.RegisterAdapter<ICurrentUser, UserTenantContext>((ctx, currentUser) => new UserTenantContext(
            ctx.Resolve<ICurrentTenant>(),
            currentUser
        ));
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }


    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AcademicApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(VerbClassApplicationModule).Assembly);
        });
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.Use(async (httpContext, next) =>
        {
            if (IsBlockedPagePath(httpContext.Request.Path))
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next();
        });
        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        app.UseMultiTenancy();

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "VerbClass API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
