using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoldRush.Core.Models.Common
{
    public abstract class GuidTenantGuidBaseEntity : GuidBaseEntity, ITenantEntity<Guid, Guid>
    {
        [ForeignKey(name: "Tenant")]
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool Active { get; set; } = true;
        public bool Deleted { get; set; } = false;
        public DateTime? DateDeleted { get; set; }
    }
}