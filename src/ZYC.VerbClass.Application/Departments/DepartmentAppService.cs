using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using ZYC.VerbClass.Application.Contracts.Departments;
using ZYC.VerbClass.Domain.DepartmentAssignments;
using ZYC.VerbClass.Domain.Departments;
using ZYC.VerbClass.Domain.Shared;
using DomainDepartmentManager = ZYC.VerbClass.Domain.Departments.DepartmentManager;

namespace ZYC.VerbClass.Application.Departments;

[Authorize(VerbClassPermissions.Departments.Access)]
public class DepartmentAppService : VerbClassAppService, IDepartmentAppService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRepository<DepartmentAssignment, Guid> _departmentAssignmentRepository;
    private readonly DomainDepartmentManager _departmentManager;

    public DepartmentAppService(
        IDepartmentRepository departmentRepository,
        IRepository<DepartmentAssignment, Guid> departmentAssignmentRepository,
        DomainDepartmentManager departmentManager)
    {
        _departmentRepository = departmentRepository;
        _departmentAssignmentRepository = departmentAssignmentRepository;
        _departmentManager = departmentManager;
    }

    public async Task<DepartmentPermissionsDto> GetPermissionsAsync()
    {
        return await GetPermissionsInternalAsync();
    }

    public async Task<DepartmentListItemDto[]> GetListAsync()
    {
        var departments = await _departmentRepository.GetListAsync();
        var currentUserCounts = await BuildCurrentUserCountsAsync();
        return DepartmentAppServiceSupport.BuildListItems(departments, currentUserCounts);
    }

    public async Task<DepartmentDetailDto> GetAsync(Guid departmentId)
    {
        var departments = await _departmentRepository.GetListAsync();
        var department = departments.FirstOrDefault(x => x.Id == departmentId)
            ?? throw CreateDepartmentNotFoundException();

        var currentUserCounts = await BuildCurrentUserCountsAsync();
        var departmentsById = departments.ToDictionary(x => x.Id);
        var permissions = await GetPermissionsInternalAsync();

        var parentDepartmentName = department.ParentDepartmentId.HasValue &&
                                   departmentsById.TryGetValue(department.ParentDepartmentId.Value, out var parent)
            ? parent.Name
            : null;

        return VerbClassApplicationMappers.ToDepartmentDetailDto(
            department,
            parentDepartmentName,
            DepartmentAppServiceSupport.BuildDepartmentDisplayPath(department, departmentsById),
            currentUserCounts.GetValueOrDefault(department.Id),
            departments.Any(x => x.ParentDepartmentId == department.Id),
            permissions.CanCreate && department.IsActive,
            permissions.CanUpdate,
            permissions.CanDelete
        );
    }

    [Authorize(VerbClassPermissions.Departments.Update)]
    public async Task<DepartmentEditorDto> GetEditorAsync(Guid departmentId)
    {
        var department = await _departmentRepository.FindAsync(departmentId)
            ?? throw CreateDepartmentNotFoundException();

        return VerbClassApplicationMappers.ToDepartmentEditorDto(department);
    }

    public async Task<DepartmentParentOptionDto[]> GetParentOptionsAsync(Guid? departmentId = null)
    {
        var departments = await _departmentRepository.GetListAsync();
        var departmentsById = departments.ToDictionary(x => x.Id);
        var options = new List<DepartmentParentOptionDto>
        {
            VerbClassApplicationMappers.ToDepartmentParentOptionDto(null, "Root")
        };

        var excludedIds = departmentId.HasValue
            ? departments
                .Where(x => x.Id == departmentId.Value ||
                            x.Path.Contains($"/{departmentId.Value}/", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .ToHashSet()
            : new HashSet<Guid>();

        foreach (var department in DepartmentAppServiceSupport.OrderDepartments(departments))
        {
            if (excludedIds.Contains(department.Id))
            {
                continue;
            }

            options.Add(VerbClassApplicationMappers.ToDepartmentParentOptionDto(
                department.Id,
                DepartmentAppServiceSupport.BuildDepartmentDisplayPath(department, departmentsById)));
        }

        return options.ToArray();
    }

    [Authorize(VerbClassPermissions.Departments.Create)]
    public async Task<DepartmentCommandResultDto> CreateAsync(CreateDepartmentInput input)
    {
        ValidateInput(input);

        try
        {
            var department = await _departmentManager.CreateAsync(
                input.Code,
                input.Name,
                input.ParentDepartmentId,
                input.ShortName,
                input.Sort,
                input.CanAssignUsers,
                input.EffectiveFrom,
                input.EffectiveTo
            );

            if (!input.IsActive)
            {
                department.Disable();
            }

            await _departmentRepository.InsertAsync(department, true);

            return VerbClassApplicationMappers.ToDepartmentCommandResultDto(department);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    [Authorize(VerbClassPermissions.Departments.Update)]
    public async Task<DepartmentCommandResultDto> UpdateAsync(Guid departmentId, UpdateDepartmentInput input)
    {
        ValidateInput(input);

        try
        {
            var department = await _departmentRepository.FindAsync(departmentId)
                ?? throw CreateDepartmentNotFoundException();

            var parentChanged = department.ParentDepartmentId != input.ParentDepartmentId;

            if (!string.Equals(department.Code, input.Code, StringComparison.Ordinal))
            {
                await _departmentManager.ChangeCodeAsync(department, input.Code);
            }

            if (parentChanged)
            {
                await _departmentManager.ChangeParentAsync(department, input.ParentDepartmentId);
            }

            department.SetName(input.Name);
            department.SetShortName(input.ShortName);
            department.SetSort(input.Sort);
            department.SetCanAssignUsers(input.CanAssignUsers);
            department.SetEffectivePeriod(input.EffectiveFrom, input.EffectiveTo);

            if (input.IsActive)
            {
                department.Enable();
            }
            else
            {
                department.Disable();
            }

            await _departmentRepository.UpdateAsync(department, true);

            if (parentChanged)
            {
                await RefreshDescendantPathsAsync(department);
            }

            return VerbClassApplicationMappers.ToDepartmentCommandResultDto(department);
        }
        catch (BusinessException ex)
        {
            throw CreateValidationException(ex);
        }
    }

    [Authorize(VerbClassPermissions.Departments.Delete)]
    public async Task<DepartmentCommandResultDto> DeleteAsync(Guid departmentId)
    {
        var department = await _departmentRepository.FindAsync(departmentId)
            ?? throw CreateDepartmentNotFoundException();

        var childDepartments = await _departmentRepository.GetChildrenAsync(departmentId);
        if (childDepartments.Count > 0)
        {
            throw new UserFriendlyException("Delete child departments first.");
        }

        var assignments = await _departmentAssignmentRepository.GetListAsync(x => x.DepartmentId == departmentId);
        if (assignments.Count > 0)
        {
            throw new UserFriendlyException("Remove department assignments before deleting this department.");
        }

        await _departmentRepository.DeleteAsync(department, true);

        return VerbClassApplicationMappers.ToDepartmentCommandResultDto(department);
    }

    public async Task<Guid[]> GetExistingIdsAsync(Guid[] departmentIds)
    {
        var normalizedDepartmentIds = departmentIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedDepartmentIds.Length == 0)
        {
            return [];
        }

        var departments = await _departmentRepository.GetListAsync(x => normalizedDepartmentIds.Contains(x.Id));
        return departments
            .Select(x => x.Id)
            .ToArray();
    }

    public async Task<UserDepartmentOptionDto[]> GetUserDepartmentOptionsAsync(Guid[] selectedDepartmentIds)
    {
        var selectedDepartmentIdSet = selectedDepartmentIds
            .Where(x => x != Guid.Empty)
            .ToHashSet();

        var departments = await _departmentRepository.GetListAsync();
        if (departments.Count == 0)
        {
            return [];
        }

        var departmentsById = departments.ToDictionary(x => x.Id);
        return DepartmentAppServiceSupport.OrderDepartments(departments)
            .Where(department =>
            {
                var isSelectable = department.IsActive && department.CanAssignUsers;
                return isSelectable || selectedDepartmentIdSet.Contains(department.Id);
            })
            .Select(department =>
            {
                var label = DepartmentAppServiceSupport.BuildDepartmentDisplayPath(department, departmentsById);
                var isSelectable = department.IsActive && department.CanAssignUsers;
                if (!isSelectable && selectedDepartmentIdSet.Contains(department.Id))
                {
                    label += " (current unavailable department)";
                }

                return VerbClassApplicationMappers.ToUserDepartmentOptionDto(department.Id, label);
            })
            .ToArray();
    }

    public async Task<UserDepartmentDisplayItemDto[]> GetUserDepartmentDisplayItemsAsync(Guid userId)
    {
        var assignments = await _departmentAssignmentRepository.GetListAsync(x => x.UserId == userId);
        if (assignments.Count == 0)
        {
            return [];
        }

        var departments = await _departmentRepository.GetListAsync();
        var departmentsById = departments.ToDictionary(x => x.Id);
        return DepartmentAppServiceSupport.BuildDepartmentDisplayItems(
            assignments,
            DateTime.UtcNow.Date,
            departmentsById
        );
    }

    public async Task<UserDepartmentSummaryDto[]> GetUserDepartmentSummariesAsync(Guid[] userIds)
    {
        var normalizedUserIds = userIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedUserIds.Length == 0)
        {
            return [];
        }

        var assignments = await _departmentAssignmentRepository.GetListAsync(x => normalizedUserIds.Contains(x.UserId));
        if (assignments.Count == 0)
        {
            return [];
        }

        var departments = await _departmentRepository.GetListAsync();
        var departmentsById = departments.ToDictionary(x => x.Id);
        var effectiveDate = DateTime.UtcNow.Date;

        return assignments
            .GroupBy(x => x.UserId)
            .Select(group =>
            {
                var items = DepartmentAppServiceSupport.BuildDepartmentDisplayItems(
                    group,
                    effectiveDate,
                    departmentsById
                );

                return VerbClassApplicationMappers.ToUserDepartmentSummaryDto(
                    group.Key,
                    DepartmentAppServiceSupport.BuildDepartmentSummary(items));
            })
            .ToArray();
    }

    private async Task<IReadOnlyDictionary<Guid, int>> BuildCurrentUserCountsAsync()
    {
        var assignments = await _departmentAssignmentRepository.GetListAsync();
        if (assignments.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var effectiveDate = DateTime.UtcNow.Date;
        return assignments
            .GroupBy(x => x.UserId)
            .Select(x => SelectDepartmentAssignment(x, effectiveDate))
            .Where(x => x is not null)
            .GroupBy(x => x!.DepartmentId)
            .ToDictionary(x => x.Key, x => x.Count());
    }

    private async Task RefreshDescendantPathsAsync(Department department)
    {
        var children = await _departmentRepository.GetChildrenAsync(department.Id);
        foreach (var child in children)
        {
            child.SetPath($"{department.Path}{child.Id}/");
            await _departmentRepository.UpdateAsync(child, true);
            await RefreshDescendantPathsAsync(child);
        }
    }

    private static DepartmentAssignment? SelectDepartmentAssignment(
        IEnumerable<DepartmentAssignment> assignments,
        DateTime effectiveDate)
    {
        var departmentAssignments = assignments as DepartmentAssignment[] ?? assignments.ToArray();
        return departmentAssignments
                   .Where(x => x.EffectiveFrom.Date <= effectiveDate &&
                               (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= effectiveDate))
                   .OrderByDescending(x => x.IsPrimary)
                   .ThenByDescending(x => x.EffectiveFrom)
                   .FirstOrDefault() ??
               departmentAssignments
                   .OrderByDescending(x => x.IsPrimary)
                   .ThenByDescending(x => x.EffectiveFrom)
                   .FirstOrDefault();
    }

    private async Task<DepartmentPermissionsDto> GetPermissionsInternalAsync()
    {
        return VerbClassApplicationMappers.ToDepartmentPermissionsDto(
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Departments.Create),
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Departments.Update),
            await AuthorizationService.IsGrantedAsync(VerbClassPermissions.Departments.Delete)
        );
    }

    private static void ValidateInput(DepartmentInputBase input)
    {
        var validationErrors = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), validationErrors, true);

        if (input.EffectiveFrom.HasValue &&
            input.EffectiveTo.HasValue &&
            input.EffectiveFrom.Value.Date > input.EffectiveTo.Value.Date)
        {
            validationErrors.Add(new ValidationResult(
                "Effective to must be later than effective from.",
                [nameof(input.EffectiveTo)]
            ));
        }

        if (validationErrors.Count > 0)
        {
            throw new AbpValidationException("Department input is invalid.", validationErrors);
        }
    }

    private static Exception CreateValidationException(BusinessException ex)
    {
        var message = ex.Code switch
        {
            DepartmentErrorCodes.CodeAlreadyExists => "Department code already exists.",
            DepartmentErrorCodes.ParentCannotBeSelf => "A department cannot be its own parent.",
            DepartmentErrorCodes.ParentNotFound => "Parent department was not found.",
            DepartmentErrorCodes.ParentIsInactive => "Parent department must be active.",
            DepartmentErrorCodes.InvalidEffectivePeriod => "Effective to must be later than effective from.",
            DepartmentErrorCodes.CircularHierarchy => "You cannot move a department under its own descendant.",
            _ => string.IsNullOrWhiteSpace(ex.Message) ? "Department operation failed." : ex.Message
        };

        var fieldName = ex.Code switch
        {
            DepartmentErrorCodes.CodeAlreadyExists => nameof(DepartmentInputBase.Code),
            DepartmentErrorCodes.ParentCannotBeSelf or
                DepartmentErrorCodes.ParentNotFound or
                DepartmentErrorCodes.ParentIsInactive or
                DepartmentErrorCodes.CircularHierarchy => nameof(DepartmentInputBase.ParentDepartmentId),
            DepartmentErrorCodes.InvalidEffectivePeriod => nameof(DepartmentInputBase.EffectiveTo),
            _ => null
        };

        if (fieldName is null)
        {
            return new UserFriendlyException(message, innerException: ex);
        }

        return new AbpValidationException(
            message,
            [new ValidationResult(message, [fieldName])]
        );
    }

    private static UserFriendlyException CreateDepartmentNotFoundException()
    {
        return new UserFriendlyException("Department was not found.");
    }
}
