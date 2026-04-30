using Volo.Abp.AuditLogging;

namespace ZYC.VerbClass.Web.Pages.Admin.AuditLogPartials;

public partial class AuditLogInfoModel
{
    public class EntityChangeInfo
    {
        public EntityChangeInfo(EntityChange change, EntityPropertyChange[] propertyChanges)
        {
            Change = change;
            PropertyChanges = propertyChanges;
        }

        public EntityChange Change { get; }

        public EntityPropertyChange[] PropertyChanges { get; }
    }
}