using System.Collections.Generic;

namespace GoldRush.Core.Audit
{
    public class AuditChange
    {
        public string Timestamp { get; set; }
        public string Action { get; set; }
        public List<AuditDelta>  Changes { get; set; }

        public AuditChange()
        {
            Changes = new List<AuditDelta>();
        }
    }
}