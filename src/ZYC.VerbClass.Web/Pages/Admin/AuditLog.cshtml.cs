using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;
using ZYC.VerbClass.Domain.Shared;
using ZYC.VerbClass.Web.Pages.Admin.AuditLogPartials;

namespace ZYC.VerbClass.Web.Pages.Admin;

[Authorize(VerbClassPermissions.AuditLogs.Access)]
public class AuditLogModel : VerbClassPageModel
{
    private const int DefaultPageSize = 50;

    private readonly IRepository<AuditLog, Guid> _auditLogRepository;

    public AuditLogModel(
        ILifetimeScope lifetimeScope,
        IRepository<AuditLog, Guid> auditLogRepository) : base(lifetimeScope)
    {
        _auditLogRepository = auditLogRepository;
    }

    public AuditLog[] AuditLogs { get; set; } = [];

    public long TotalCount { get; set; }

    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)] public Guid? AuditLogId { get; set; }

    public AuditLogInfoModel? SelectedAuditLog { get; private set; }

    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)DefaultPageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public int PreviousPageNumber => PageNumber - 1;

    public int NextPageNumber => PageNumber + 1;

    public long PageStart => TotalCount == 0 ? 0 : ((long)(PageNumber - 1) * DefaultPageSize) + 1;

    public long PageEnd => TotalCount == 0 ? 0 : PageStart + AuditLogs.LongLength - 1;

    protected override string PageTitle => "Audit Log";

    public async Task OnGetAsync()
    {
        var queryable = await _auditLogRepository.GetQueryableAsync();
        TotalCount = await queryable.LongCountAsync();

        var lastPageNumber = Math.Max(1, TotalPages);
        PageNumber = Math.Clamp(PageNumber, 1, lastPageNumber);

        AuditLogs = await queryable
            .OrderByDescending(x => x.ExecutionTime)
            .Skip((PageNumber - 1) * DefaultPageSize)
            .Take(DefaultPageSize)
            .ToArrayAsync();

        if (!AuditLogId.HasValue || AuditLogs.All(x => x.Id != AuditLogId.Value))
        {
            return;
        }

        SelectedAuditLog = await BuildAuditLogInfoModelAsync(AuditLogId.Value);
    }

    public async Task<PartialViewResult> OnGetSelectAsync(Guid auditLogId)
    {
        return await BuildAuditLogInfoPartialAsync(auditLogId);
    }

    public async Task<PartialViewResult> OnPostSelectAsync(Guid id)
    {
        return await BuildAuditLogInfoPartialAsync(id);
    }

    private async Task<PartialViewResult> BuildAuditLogInfoPartialAsync(Guid auditLogId)
    {
        var model = await BuildAuditLogInfoModelAsync(auditLogId);
        return Partial("~/Pages/Admin/AuditLogPartials/_AuditLogInfo.cshtml", model);
    }

    private async Task<AuditLogInfoModel> BuildAuditLogInfoModelAsync(Guid auditLogId)
    {
        return await ResolveModelAsync<AuditLogInfoModel>(new TypedParameter(typeof(Guid), auditLogId));
    }
}
