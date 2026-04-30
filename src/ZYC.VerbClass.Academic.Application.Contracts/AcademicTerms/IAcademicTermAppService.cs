using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;

public interface IAcademicTermAppService : IApplicationService
{
    Task<AcademicTermListItemDto[]> GetListAsync();

    Task<AcademicTermDetailDto> GetAsync(Guid termId);

    Task<AcademicTermCommandResultDto> CreateAsync(CreateAcademicTermInput input);

    Task<AcademicTermCommandResultDto> UpdateAsync(Guid termId, UpdateAcademicTermInput input);

    Task<AcademicTermCommandResultDto> LockAsync(Guid termId);

    Task<AcademicTermCommandResultDto> DeleteAsync(Guid termId);
}
