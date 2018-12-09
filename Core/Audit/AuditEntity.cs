using System;
using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Audit
{
    public class AuditEntity : IBaseEntity<int>
    {
        public string CompanyId { get; set; }
        public string TenantId { get; set; }
        public string Action { get; set; }
        public string Source { get; set; }
        public string Namespace { get; set; }
        public string Model { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Key { get; set; }
        public string PreviousValue { get; set; }
        public string CurrentValue { get; set; }
        public string  Changes { get; set; }
        public int Id { get; set; }
        public byte[] ConcurrencyStamp { get; set; }
        public DateTime? ConcurrencyTimeStamp { get; set; }
    }
}