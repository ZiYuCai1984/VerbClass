using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicSettings;

public interface IAcademicSettingsAppService : IApplicationService
{
    Task<AcademicSettingsDto> GetAsync();

    Task<Guid?> GetCurrentTermIdAsync();

    Task<AcademicSettingsDto> SaveCurrentTermAsync(UpdateCurrentAcademicTermInput input);
}
