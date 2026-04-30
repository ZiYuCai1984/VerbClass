using Riok.Mapperly.Abstractions;
using ZYC.VerbClass.Academic.Application.Contracts.Academic;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTerms;
using ZYC.VerbClass.Academic.Application.Contracts.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Application.Contracts.CourseDefinitions;
using ZYC.VerbClass.Academic.Application.Contracts.CourseOfferings;
using ZYC.VerbClass.Academic.Domain.AcademicTerms;
using ZYC.VerbClass.Academic.Domain.AcademicTimeTemplates;
using ZYC.VerbClass.Academic.Domain.CourseDefinitions;
using ZYC.VerbClass.Academic.Domain.CourseOfferings;

namespace ZYC.VerbClass.Academic.Application;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class AcademicApplicationDtoMapper
{
    public static partial CourseDefinitionListItemDto ToCourseDefinitionListItemDto(
        CourseDefinition courseDefinition);

    public static partial CourseDefinitionDetailDto ToCourseDefinitionDetailDto(
        CourseDefinition courseDefinition);

    public static partial CourseDefinitionOptionDto ToCourseDefinitionOptionDto(
        CourseDefinition courseDefinition);

    public static partial CourseDefinitionCommandResultDto ToCourseDefinitionCommandResultDto(
        CourseDefinition courseDefinition);

    public static partial AcademicTimeTemplateDetailDto ToAcademicTimeTemplateDetailDto(
        AcademicTimeTemplate template);

    public static partial AcademicTimeTemplateOptionDto ToAcademicTimeTemplateOptionDto(
        AcademicTimeTemplate template);

    public static partial AcademicTimeTemplateCommandResultDto ToAcademicTimeTemplateCommandResultDto(
        AcademicTimeTemplate template);

    public static partial AcademicTermDetailDto ToAcademicTermDetailDto(AcademicTerm term);

    public static partial AcademicTermCommandResultDto ToAcademicTermCommandResultDto(AcademicTerm term);

    public static partial CourseOfferingListItemDto ToCourseOfferingListItemDto(CourseOffering courseOffering);

    public static partial CourseOfferingCommandResultDto ToCourseOfferingCommandResultDto(
        CourseOffering courseOffering);

    public static partial AcademicPeriodDefinitionDto ToAcademicPeriodDefinitionDto(
        TimeTemplatePeriodDefinition period);

    public static partial AcademicPeriodDefinitionDto ToAcademicPeriodDefinitionDto(
        TermPeriodDefinition period);

    public static partial AcademicPeriodDefinitionDto[] ToAcademicPeriodDefinitionDtos(
        IEnumerable<TimeTemplatePeriodDefinition> periods);

    public static partial AcademicPeriodDefinitionDto[] ToAcademicPeriodDefinitionDtos(
        IEnumerable<TermPeriodDefinition> periods);

    public static partial AcademicScheduleSlotDto ToAcademicScheduleSlotDto(
        CourseOfferingScheduleSlot scheduleSlot);

    public static partial AcademicScheduleSlotDto[] ToAcademicScheduleSlotDtos(
        IEnumerable<CourseOfferingScheduleSlot> scheduleSlots);

    private static DateOnly MapDateTimeToDateOnly(DateTime value)
    {
        return DateOnly.FromDateTime(value);
    }
}
