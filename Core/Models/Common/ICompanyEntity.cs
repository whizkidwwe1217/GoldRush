namespace GoldRush.Core.Models.Common
{
    public interface ICompanyEntity<TCompanyKey, TTenantKey, TKey> : ITenantEntity<TTenantKey, TKey>
    {
        TCompanyKey CompanyId { get; set; }
    }
}