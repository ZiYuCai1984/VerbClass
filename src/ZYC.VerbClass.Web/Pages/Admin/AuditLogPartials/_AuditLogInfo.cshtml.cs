using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ZYC.VerbClass.Web.Pages.Admin.AuditLogPartials;

public partial class AuditLogInfoModel : VerbClassModel, ITransientDependency
{
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;

    public AuditLogInfoModel(
        Guid id,
        IRepository<AuditLog, Guid> auditLogRepository)
    {
        Id = id;
        _auditLogRepository = auditLogRepository;
    }

    public Guid Id { get; }

    public AuditLog? AuditLog { get; private set; }

    public AuditLogAction[] Actions { get; private set; } = [];

    public EntityChangeInfo[] EntityChanges { get; private set; } = [];

    public override async Task InitializeAsync()
    {
        var queryable = await _auditLogRepository.WithDetailsAsync(x => x.Actions, x => x.EntityChanges);
        AuditLog = await queryable
            .Include(x => x.EntityChanges)
            .ThenInclude(x => x.PropertyChanges)
            .FirstOrDefaultAsync(x => x.Id == Id);

        if (AuditLog is null)
        {
            return;
        }

        Actions = AuditLog.Actions
            .OrderByDescending(x => x.ExecutionTime)
            .ToArray();

        EntityChanges = AuditLog.EntityChanges
            .OrderByDescending(x => x.ChangeTime)
            .Select(x => new EntityChangeInfo(
                x,
                x.PropertyChanges
                    .OrderBy(y => y.PropertyName)
                    .ToArray()
            ))
            .ToArray();
    }
}
