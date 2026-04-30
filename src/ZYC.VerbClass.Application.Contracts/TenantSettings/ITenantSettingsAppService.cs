using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Application.Contracts.TenantSettings;

public interface ITenantSettingsAppService : IApplicationService
{
    Task<TenantSettingsDto> GetAsync();

    Task<TenantPasswordPolicyDto> SavePasswordPolicyAsync(UpdateTenantPasswordPolicyInput input);
}
