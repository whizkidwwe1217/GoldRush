using System;
using GoldRush.Core.Models.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GoldRush.Core.Models
{
    public class Tenant : GuidBaseEntity, ICanActivate, ICanAudit, ICanSoftDelete
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConnectionString { get; set; }
        public string HostName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseEngine Engine { get; set; }
        public string Edition { get; set; }
        public bool IsIsolated { get; set; }
        public bool IsTenantAdministrator { get; set; }
        public string DeploymentStatus { get; set; }
        public Subscription Subscription { get; set; }
        public int? SubscriptionId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string Theme { get; set; }
    }
}