using Autofac.Core.Lifetime;
using ZYC.VerbClass.Web.Abstractions.Toast;

namespace ZYC.VerbClass.Web.Core;

public abstract class VerbClassModel
{
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}