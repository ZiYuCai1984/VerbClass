using Microsoft.AspNetCore.Mvc.Rendering;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.Shared;

namespace ZYC.VerbClass.Web.Pages.Academic;

internal static class CoursePageMapping
{
    public static CourseDefinitionListItemModel[] BuildDefinitionListItems(
        IEnumerable<CourseDefinitionListItemDto> definitions,
        IEnumerable<CourseOfferingListItemDto> offerings)
    {
        var metricsByDefinitionId = offerings
            .GroupBy(x => x.CourseDefinitionId)
            .ToDictionary(
                x => x.Key,
                x => new CourseDefinitionOfferingMetrics(
                    x.Count(),
                    x.Count(y => y.Status is CourseOfferingStatus.Active or CourseOfferingStatus.Published),
                    x.Count(y => y.IsLocked))
            );

        return definitions
            .Select(x =>
            {
                metricsByDefinitionId.TryGetValue(x.Id, out var metrics);

                return new CourseDefinitionListItemModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    ShortName = x.ShortName,
                    IsActive = x.IsActive,
                    OfferingCount = metrics?.OfferingCount ?? 0,
                    ActiveOfferingCount = metrics?.ActiveOfferingCount ?? 0,
                    LockedOfferingCount = metrics?.LockedOfferingCount ?? 0
                };
            })
            .ToArray();
    }

    public static CourseDefinitionDetailModel? BuildDefinitionDetail(
        Guid? activeCourseDefinitionId,
        IEnumerable<CourseDefinitionListItemDto> definitions,
        IEnumerable<CourseOfferingListItemDto> offerings)
    {
        if (!activeCourseDefinitionId.HasValue)
        {
            return null;
        }

        var definition = definitions.FirstOrDefault(x => x.Id == activeCourseDefinitionId.Value);
        if (definition is null)
        {
            return null;
        }

        return new CourseDefinitionDetailModel
        {
            Id = definition.Id,
            Code = definition.Code,
            Name = definition.Name,
            ShortName = definition.ShortName,
            Description = definition.Description,
            IsActive = definition.IsActive,
            Offerings = offerings
                .Where(x => x.CourseDefinitionId == definition.Id)
                .ToArray()
        };
    }

    public static IReadOnlyList<SelectListItem> BuildDefinitionOptions(
        IEnumerable<CourseDefinitionListItemDto> definitions)
    {
        return definitions
            .Select(x => new SelectListItem(
                $"{x.Code} · {x.Name}",
                x.Id.ToString()))
            .ToArray();
    }

    public static CourseOfferingListItemModel[] BuildOfferingListItems(
        IEnumerable<CourseOfferingListItemDto> offerings)
    {
        return offerings
            .Select(x => new CourseOfferingListItemModel
            {
                Id = x.Id,
                CourseDefinitionId = x.CourseDefinitionId,
                CourseDefinitionCode = x.CourseDefinitionCode,
                CourseDefinitionName = x.CourseDefinitionName,
                AcademicYear = x.AcademicYear,
                TermName = x.TermName,
                ScheduleDayOfWeek = x.ScheduleDayOfWeek,
                ScheduleStartTime = x.ScheduleStartTime,
                ScheduleEndTime = x.ScheduleEndTime,
                ScheduleLocation = x.ScheduleLocation,
                Status = x.Status,
                IsLocked = x.IsLocked
            })
            .ToArray();
    }

    public static CourseOfferingDetailModel? BuildOfferingDetail(
        Guid? activeCourseOfferingId,
        IEnumerable<CourseDefinitionListItemDto> definitions,
        IEnumerable<CourseOfferingListItemDto> offerings)
    {
        if (!activeCourseOfferingId.HasValue)
        {
            return null;
        }

        var offering = offerings.FirstOrDefault(x => x.Id == activeCourseOfferingId.Value);
        if (offering is null)
        {
            return null;
        }

        var definition = definitions.FirstOrDefault(x => x.Id == offering.CourseDefinitionId);

        return new CourseOfferingDetailModel
        {
            Id = offering.Id,
            CourseDefinitionId = offering.CourseDefinitionId,
            CourseDefinitionCode = offering.CourseDefinitionCode,
            CourseDefinitionName = offering.CourseDefinitionName,
            CourseDefinitionShortName = definition?.ShortName,
            CourseDefinitionDescription = definition?.Description,
            IsCourseDefinitionActive = definition?.IsActive ?? false,
            AcademicYear = offering.AcademicYear,
            TermName = offering.TermName,
            ScheduleDayOfWeek = offering.ScheduleDayOfWeek,
            ScheduleStartTime = offering.ScheduleStartTime,
            ScheduleEndTime = offering.ScheduleEndTime,
            ScheduleLocation = offering.ScheduleLocation,
            EnrollmentStartsAt = offering.EnrollmentStartsAt,
            EnrollmentEndsAt = offering.EnrollmentEndsAt,
            Status = offering.Status,
            IsLocked = offering.IsLocked
        };
    }

    private sealed record CourseDefinitionOfferingMetrics(
        int OfferingCount,
        int ActiveOfferingCount,
        int LockedOfferingCount);
}
