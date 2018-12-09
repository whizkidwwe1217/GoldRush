using System;

namespace GoldRush.Core.Models.Common
{
    public abstract class GuidCompanyGuidBaseEntity : GuidTenantGuidBaseEntity, ICompanyEntity<Guid?, Guid, Guid>
    {
        public Guid? CompanyId { get; set; }
        public Company Company { get; set; }
    }
}