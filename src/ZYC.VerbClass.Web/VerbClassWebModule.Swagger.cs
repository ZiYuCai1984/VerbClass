using Microsoft.OpenApi;

namespace ZYC.VerbClass.Web;

public partial class VerbClassWebModule
{
    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "VerbClass API", Version = "v1" });
                options.DocInclusionPredicate((_, _) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }
}