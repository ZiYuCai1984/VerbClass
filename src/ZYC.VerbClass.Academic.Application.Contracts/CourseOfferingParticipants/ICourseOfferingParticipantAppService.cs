using Volo.Abp.Application.Services;

namespace ZYC.VerbClass.Academic.Application.Contracts.CourseOfferingParticipants;

public interface ICourseOfferingParticipantAppService : IApplicationService
{
    Task<CourseOfferingParticipantListItemDto[]> GetListAsync(Guid courseOfferingId);

    Task<CourseOfferingParticipantUserOptionDto[]> GetAssignableUsersAsync(Guid courseOfferingId);

    Task<CourseOfferingParticipantCommandResultDto> AddAsync(AddCourseOfferingParticipantInput input);

    Task<CourseOfferingParticipantCommandResultDto> ChangeRoleAsync(
        Guid participantId,
        ChangeCourseOfferingParticipantRoleInput input);

    Task<CourseOfferingParticipantCommandResultDto> RemoveAsync(Guid participantId);
}
