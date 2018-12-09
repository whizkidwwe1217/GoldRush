using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoldRush.Core.Models.Common;
using GoldRush.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoldRush.Core.Controllers
{
    public interface IServiceController<TKey, TEntity> where TEntity : class, IBaseEntity<TKey>, new()
    {
        IService<TKey, TEntity> Service { get; }
        Task<ActionResult<List<TEntity>>> List(CancellationToken cancellationToken = default(CancellationToken));
        Task<ActionResult<TEntity>> Get(TKey id, CancellationToken cancellationToken = default(CancellationToken));
        Task<IActionResult> Create([FromBody] TEntity entity);
        Task<IActionResult> Update(TKey id, [FromBody] TEntity entity);
        Task<IActionResult> Delete(TKey id);
        Task<ActionResult<SearchResponseData<TEntity>>> SearchQuery(
            int? currentPage = 1, int? pageSize = 100, string filter = "", string sort = "", string fields = "", 
            CancellationToken cancellationToken = default(CancellationToken));
        Task<ActionResult<SearchResponseData<TEntity>>> Search(SearchParams parameter, CancellationToken cancellationToken = default(CancellationToken));
    }
}