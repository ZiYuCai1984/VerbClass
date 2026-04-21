using Microsoft.Extensions.Configuration;
using OpenIddict.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.Uow;

namespace ZYC.VerbClass.Domain.Data;

/* Creates initial data that is needed to property run the application
 * and make client-to-server communication possible.
 */
public class OpenIddictDataSeedContributor : OpenIddictDataSeedContributorBase, IDataSeedContributor,
    ITransientDependency
{
    public OpenIddictDataSeedContributor(
        IConfiguration configuration,
        IOpenIddictApplicationRepository openIddictApplicationRepository,
        IAbpApplicationManager applicationManager,
        IOpenIddictScopeRepository openIddictScopeRepository,
        IOpenIddictScopeManager scopeManager)
        : base(configuration, openIddictApplicationRepository, applicationManager, openIddictScopeRepository,
            scopeManager)
    {
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateScopesAsync();
        await CreateApplicationsAsync();
    }

    private async Task CreateScopesAsync()
    {
        await CreateScopesAsync(new OpenIddictScopeDescriptor
        {
            Name = "VerbClass",
            DisplayName = "VerbClass API",
            Resources = { "VerbClass" }
        });
    }

    private async Task CreateApplicationsAsync()
    {
        var commonScopes = new List<string>
        {
            OpenIddictConstants.Permissions.Scopes.Address,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
            "VerbClass"
        };

        var configurationSection = Configuration.GetSection("OpenIddict:Applications");


        //Console Test / Angular Client
        var consoleAndAngularClientId = configurationSection["VerbClass_App:ClientId"];
        if (!consoleAndAngularClientId.IsNullOrWhiteSpace())
        {
            var consoleAndAngularClientRootUrl = configurationSection["VerbClass_App:RootUrl"]?.TrimEnd('/');
            await CreateOrUpdateApplicationAsync(
                OpenIddictConstants.ApplicationTypes.Web,
                consoleAndAngularClientId!,
                OpenIddictConstants.ClientTypes.Public,
                OpenIddictConstants.ConsentTypes.Implicit,
                "Console Test / Angular Application",
                null,
                [
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.ClientCredentials,
                    OpenIddictConstants.GrantTypes.RefreshToken,
                    "LinkLogin",
                    "Impersonation"
                ],
                commonScopes,
                [consoleAndAngularClientRootUrl],
                [consoleAndAngularClientRootUrl],
                consoleAndAngularClientRootUrl,
                "/images/clients/angular.svg"
            );
        }

        // Swagger Client
        var swaggerClientId = configurationSection["VerbClass_Swagger:ClientId"];
        if (!swaggerClientId.IsNullOrWhiteSpace())
        {
            var swaggerRootUrl = configurationSection["VerbClass_Swagger:RootUrl"]?.TrimEnd('/');

            await CreateOrUpdateApplicationAsync(
                OpenIddictConstants.ApplicationTypes.Web,
                swaggerClientId!,
                OpenIddictConstants.ClientTypes.Public,
                OpenIddictConstants.ConsentTypes.Implicit,
                "Swagger Application",
                null,
                [OpenIddictConstants.GrantTypes.AuthorizationCode],
                commonScopes,
                [$"{swaggerRootUrl}/swagger/oauth2-redirect.html"],
                clientUri: swaggerRootUrl?.EnsureEndsWith('/') + "swagger",
                logoUri: "/images/clients/swagger.svg"
            );
        }
    }
}