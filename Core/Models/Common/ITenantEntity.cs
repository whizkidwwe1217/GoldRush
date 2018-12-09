namespace GoldRush.Core.Models.Common
{
    public interface ITenantEntity<TTenantKey, TKey> : IBaseEntity<TKey>, ICanActivate, ICanSoftDelete, ICanAudit
    {
        TTenantKey TenantId { get; set; }
    }
}