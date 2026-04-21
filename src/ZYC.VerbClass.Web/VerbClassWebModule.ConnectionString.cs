using Volo.Abp.Data;

namespace ZYC.VerbClass.Web;

public partial class VerbClassWebModule
{
    private void ConfigureConnectionString()
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "VerbClass.db");

        //TODO-zyc Temp code(ConnectionStrings)
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = $"Data Source={dbPath}";
        });
    }
}