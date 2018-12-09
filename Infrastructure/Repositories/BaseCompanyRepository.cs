using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GoldRush.Core.Models;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Pipeline;
using GoldRush.Core.Repositories;
using GoldRush.Core.SequenceNumbers;
using GoldRush.Multitenancy;
using Microsoft.EntityFrameworkCore;
using StructureMap.Attributes;

namespace GoldRush.Infrastructure.Repositories
{
    public class CompanyFilter<TKey, TEntity> : RepositoryFilter<TKey, TEntity, BaseCompanyRepository<TKey, TEntity>>
        where TEntity : class, IBaseEntity<TKey>, ICanAudit, new()
    {
        public CompanyFilter(BaseCompanyRepository<TKey, TEntity> repository) : base(repository)
        {
        }

        protected override async Task<TEntity> Process(TEntity input)
        {
            object userCompanyId;
            Repository.TenantContext.Properties.TryGetValue("CompanyId", out userCompanyId);

            if (userCompanyId != null && typeof(ICompanyEntity<Guid?, Guid, TKey>).IsAssignableFrom(typeof(TEntity)))
            {
                var companyId = Guid.Parse(userCompanyId.ToString());
                ((ICompanyEntity<Guid?, Guid, TKey>)input).CompanyId = companyId;
            }
            return await Task.FromResult(input);
        }
    }

    public abstract class BaseCompanyRepository<TKey, TEntity> : BaseRepository<TKey, TEntity>
        where TEntity : class, IBaseEntity<TKey>, ICanAudit, new()
    {
        public BaseCompanyRepository(IRepositoryManager<TKey> repositoryManager) : base(repositoryManager)
        {
        }

        public virtual void ConfigurePipelines(Pipeline<TEntity> pipeline, EntityState state)
        {
            pipeline.Register(new CompanyFilter<TKey, TEntity>(this));
        }

        [SetterProperty]
        public TenantContext<Tenant> TenantContext { get; set; }

        public async Task<string> GetNextSequenceNumber(string name)
        {
            object userCompanyId;
            TenantContext.Properties.TryGetValue("CompanyId", out userCompanyId);

            if (userCompanyId != null && typeof(ICompanyEntity<Guid?, Guid, TKey>).IsAssignableFrom(typeof(TEntity)))
            {
                var companyId = Guid.Parse(userCompanyId.ToString());
                var sequence = await DbContext.Set<SequenceNumber>()
                    .Where(e => e.TenantId == RepositoryManager.Tenant.Id
                        && e.CompanyId == companyId
                        && e.Name == name
                        && e.Active
                        && !e.Deleted)
                    .FirstOrDefaultAsync();
                if (sequence != null)
                {
                    var generator = new SequenceNumberGenerator()
                        .SetPrefix(sequence.Prefix)
                        .SetSuffix(sequence.Suffix)
                        .SetLeftPadding(sequence.LeftPadding, sequence.LeftPaddingChar)
                        .SetRightPadding(sequence.RightPadding, sequence.RightPaddingChar)
                        .SetEndCyclePosition(sequence.EndCyclePosition, sequence.CycleSequence)
                        .SetResetValue(sequence.ResetValue)
                        .SetStartingValue(sequence.StartingValue);
                    var sequenceNo = generator.GenerateSequenceNumber();
                    sequence.StartingValue = generator.StartingValue;
                    DbContext.Set<SequenceNumber>().Update(sequence);
                    return await Task.FromResult(sequenceNo);
                }
            }

            return null;
        }

        protected override string GetCompanyIdString()
        {
            object userCompanyId;
            TenantContext.Properties.TryGetValue("CompanyId", out userCompanyId);
            return userCompanyId.ToString();
        }

        protected override IQueryable<TEntity> GetTenantEntity()
        {
            var condition = base.GetTenantEntity();

            if (condition != null && TenantContext != null)
            {
                object userCompanyId;
                TenantContext.Properties.TryGetValue("CompanyId", out userCompanyId);
                if (userCompanyId != null && typeof(ICompanyEntity<Guid?, Guid, TKey>).IsAssignableFrom(typeof(TEntity)))
                {
                    var companyId = Guid.Parse(userCompanyId.ToString());
                    condition = base.GetTenantEntity()
                        .Cast<ICompanyEntity<Guid?, Guid, TKey>>()
                        .Where(e => e.CompanyId == companyId)
                        .Cast<TEntity>();
                }
            }
            return condition;
        }
    }
}