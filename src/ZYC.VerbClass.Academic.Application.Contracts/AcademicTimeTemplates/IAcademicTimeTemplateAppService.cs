using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;

public interface IAcademicTimeTemplateAppService : IApplicationService
{
    Task<AcademicTimeTemplateListItemDto[]> GetListAsync();

    Task<AcademicTimeTemplateDetailDto> GetAsync(Guid templateId);

    Task<AcademicTimeTemplateOptionDto[]> GetOptionsAsync();

    Task<AcademicTimeTemplateCommandResultDto> CreateAsync(CreateAcademicTimeTemplateInput input);

    Task<AcademicTimeTemplateCommandResultDto> UpdateAsync(Guid templateId, UpdateAcademicTimeTemplateInput input);

    Task<AcademicTimeTemplateCommandResultDto> DeleteAsync(Guid templateId);
}
