using System;

namespace GoldRush.Core.Models.Common
{
    public abstract class IntTenantBaseEntity : IntBaseEntity, ITenantEntity<int, int>
    {
        public int TenantId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool Active { get; set; } = true;
        public bool Deleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }
    }
}