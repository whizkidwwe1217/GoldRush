using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GoldRush.Core
{
    public static class SearchExtensions
    {
        public static IQueryable<TEntity> Search<TEntity, TKey>(
            this IQueryable<TEntity> query,
            SearchParams parameters)
            where TEntity : class, new()
        {
            return query.Search<TEntity, TKey>(parameters.Filter, parameters.Sort);
        }

        public static async Task<SearchResponseData> ToListAsync<TEntity>(
            this IQueryable<object> query, 
            SearchParams parameters,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await query.ToListAsync<object>(
                parameters.CurrentPage, parameters.PageSize, 
                parameters.Fields, cancellationToken);
        }

        public static async Task<SearchResponseData> ToListAsync<TEntity>(
            this IQueryable<object> query, 
            int? currentPage = 1, 
            int? pageSize = 100,
            string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ToSearchResponseDataAsync<TEntity>(
                query, currentPage, pageSize, fields, cancellationToken);
        }

        public static async Task<SearchResponseData> ToSearchResponseDataAsync<TEntity>(
            IQueryable<object> query, 
            int? currentPage = 1, 
            int? pageSize = 100, 
            string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return new SearchResponseData
            {
                total = await query.CountAsync(cancellationToken),
                data = await query
                    .Page<object>(currentPage, pageSize)
                    .Select<object>(fields)
                    .ToListAsync(cancellationToken),
                currentPage = currentPage,
                pageSize = pageSize
            };
        }

        public static async Task<SearchResponseData<TEntity>> ToListAsync<TEntity>(
            this IQueryable<TEntity> query,
            int? currentPage = 1,
            int? pageSize = 100,
            string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ToSearchResponseDataAsync(query, currentPage, pageSize, fields, cancellationToken);
        }

        public static async Task<SearchResponseData<TEntity>> ToSearchResponseDataAsync<TEntity>(
            IQueryable<TEntity> query,
            int? currentPage = 1,
            int? pageSize = 100,
            string fields = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return new SearchResponseData<TEntity>
            {
                total = await query.CountAsync(cancellationToken),
                data = await query
                    .Page<TEntity>(currentPage, pageSize)
                    .Select<TEntity>(fields)
                    .ToListAsync(cancellationToken),
                currentPage = currentPage,
                pageSize = pageSize
            };
        }

        // Field selection
        public static IQueryable<TEntity> Select<TEntity>(
            this IQueryable<TEntity> query,
            string fields = "")
        {
            if(string.IsNullOrEmpty(fields))
                return query;
            return query
                .Select(e => e.SerializeSelect<TEntity>(fields))
                .Cast<TEntity>();
        }

        // Applies paging to IQueryable<TEntity>.
        public static IQueryable<TEntity> Page<TEntity>(
            this IQueryable<TEntity> query,
            int? currentPage = 1, 
            int? pageSize = 100)
        {
            return query
                .Skip(((int)currentPage - 1) * (int)pageSize)
                .Take((int)pageSize);
        }

        // Builds an IQueryable<TEntity> based on dynamic fields selector and dynamic filtering.
        public static IQueryable<TEntity> Search<TEntity, TKey>(
            this IQueryable<TEntity> query, 
            string filter = "",
            string sort = "")
            where TEntity : class, new()
        {
            IQueryable<TEntity> data = query
                .SortBy<TEntity>(sort)
                .AsNoTracking();

            // Filter
            if(!string.IsNullOrEmpty(filter))
            {
                var predicate = DynamicExpressionBuilder.BuildFilterExpression<TEntity>(filter);
                
                if (predicate != null)
                    data = data.Where(predicate);
            }

            return data;
        }
    }
}