using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.AcademicTimetables;

public interface IAcademicTimetableAppService : IApplicationService
{
    Task<AcademicTimetableDto> GetTermTimetableAsync(Guid academicTermId);
}
