using System;

namespace GoldRush.Core.Models.Common
{
    public abstract class GuidCompanyBaseEntity<TKey> : GuidTenantBaseEntity<TKey>, ICompanyEntity<Guid?, Guid, TKey>
    {
        public Guid? CompanyId { get; set; }
        public Company Company { get; set; }
    }
}