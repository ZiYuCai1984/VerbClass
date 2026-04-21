using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.Memberships;

public interface ICourseMembershipAppService : IApplicationService
{
    Task<CourseMembershipListItemDto[]> GetListAsync(Guid courseOfferingId);

    Task<CourseMembershipCommandResultDto> JoinStudentAsync(JoinCourseMembershipInput input);

    Task<CourseMembershipCommandResultDto> DropAsync(Guid id);

    Task<CourseMembershipCommandResultDto> UpdateRoleAsync(Guid id, UpdateCourseMembershipRoleInput input);
}
