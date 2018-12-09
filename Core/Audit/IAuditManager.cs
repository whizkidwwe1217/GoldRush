using System;

namespace GoldRush.Core.Audit
{
    public interface IAuditManager<TKey>
    {
        AuditEntity Audit(AuditActions action, string tenantId, string companyId, Type model, string key, object previousObject, object currentObject);
        AuditChange DeserializeAuditChange(AuditEntity audit);
    }
}