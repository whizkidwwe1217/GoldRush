using System;

namespace GoldRush.Core.Models.Common
{
    public abstract class GuidCompanyIntBaseEntity : GuidTenantIntBaseEntity, ICompanyEntity<Guid?, Guid, int>
    {
        public Guid? CompanyId { get; set; }
        public Company Company { get; set; }
    }
}