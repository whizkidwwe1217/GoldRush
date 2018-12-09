#define DEBUG

using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;

namespace GoldRush.Core.Audit
{
    public class AuditManager<TKey> : IAuditManager<TKey>
    {
        public AuditEntity Audit(AuditActions action, string tenantId, string companyId, Type model, string key, object previousObject, object currentObject)
        {
#if DEBUG
#warning [WARNING!!!] Audit trail must exclude byte or binary fields that might contain large data. Remove this warning after implemented.
#endif
            var comparer = new CompareLogic();
            comparer.Config.MaxDifferences = 99;
            comparer.Config.IgnoreObjectTypes = true;
            comparer.Config.TypesToIgnore = new List<Type> { typeof(byte), typeof(byte[]) };
            // Test this with binaries and remove pragma warnings above.
            // comparer.Config.TypesToIgnore.AddRange(new Type[] {
            //     typeof(byte),
            //     typeof(byte[])
            // });
            comparer.Config.MembersToIgnore.AddRange(new string[] {
                "ConcurrencyStamp",
                "ConcurrencyTimeStamp",
                "DateCreated",
                "DateModified",
                "DateDeleted"
            });

            var result = comparer.Compare(previousObject, currentObject);
            var deltas = new List<AuditDelta>();

            if (result.AreEqual && action == AuditActions.Update) return null;

            foreach (var change in result.Differences)
            {
                var delta = new AuditDelta();
                delta.PropertyName = change.PropertyName;
                delta.PreviousValue = change.Object1Value;
                delta.CurrentValue = change.Object2Value;
                deltas.Add(delta);
            }

            // if(typeof(ITenantEntity<Guid, TKey>).IsAssignableFrom(model))
            // {
            //     tenantId = ((ITenantEntity<Guid, TKey>) model).TenantId.ToString(); 
            // }

            // if(typeof(ICompanyEntity<Guid, Guid, TKey>).IsAssignableFrom(model)) // Just default to GUID for company id for now.
            // {
            //     companyId = ((ICompanyEntity<Guid, Guid, TKey>) model).CompanyId.ToString(); 
            // }

            return new AuditEntity
            {
                Action = action.ToString(),
                Source = "Audit Manager",
                TimeStamp = DateTime.UtcNow,
                Model = model.Name,
                Namespace = model.Namespace,
                TenantId = tenantId,
                CompanyId = companyId,
                Key = key,
                PreviousValue = JsonConvert.SerializeObject(previousObject),
                CurrentValue = JsonConvert.SerializeObject(currentObject),
                Changes = JsonConvert.SerializeObject(deltas)
            };
        }

        public AuditChange DeserializeAuditChange(AuditEntity audit)
        {
            var change = new AuditChange();
            change.Timestamp = audit.TimeStamp.ToString();
            change.Action = audit.Action;
            change.Changes = JsonConvert.DeserializeObject<List<AuditDelta>>(audit.Changes);
            return change;
        }
    }
}