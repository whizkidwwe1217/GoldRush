using System;
using GoldRush.Core.Licensing;
using GoldRush.Core.Models.Common;

namespace GoldRush.Core.Models
{
    public class Subscription : GuidBaseEntity, ICanActivate
    {
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? SubscriptionDate { get; set; }
        public bool Active { get; set; }
        public Guid TenantId { get; set; }
    }
}