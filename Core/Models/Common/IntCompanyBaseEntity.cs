namespace GoldRush.Core.Models.Common
{
    public abstract class IntCompanyBaseEntity : IntTenantBaseEntity, ICompanyEntity<int, int, int>
    {
        public int CompanyId { get; set; }
    }
}